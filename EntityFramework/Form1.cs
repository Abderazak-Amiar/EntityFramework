namespace EntityFramework
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;

    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        private readonly BindingSource usersBinding = new BindingSource();

        // Prevent programmatic selection changes from triggering the selection handler

        private bool suppressSelectionEvents;

        // Prevent re-entrancy when applying portion

        private bool suppressPortionApply;

        public List<User> DatabaseUsers { get; private set; } = new List<User>();

        // Vente pagination fields

        private readonly int ventePageSize = 10;

        private int venteCurrentPage = 1;

        private int venteTotalPages = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Make selection predictable: full-row, single selection
            ItemList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ItemList.MultiSelect = false;
            ItemList.ReadOnly = true; // optional: prevent inline editing in grid
            ItemList.AllowUserToAddRows = false;

            // Configure columns before binding to avoid DataGridView creating its own columns
            ConfigureGridColumns();

            // Bind via a BindingSource so selection and refresh are simpler
            ItemList.DataSource = usersBinding;

            // Wire the selection change event to the existing handler
            ItemList.SelectionChanged -= ItemList_SelectedRowsChanged;
            ItemList.SelectionChanged += ItemList_SelectedRowsChanged;
            ItemList.DataBindingComplete -= ItemList_DataBindingComplete;
            ItemList.DataBindingComplete += ItemList_DataBindingComplete;

            // Ensure cell formatting for rendement to avoid mutating cell values during binding
            ItemList.CellFormatting -= ItemList_CellFormatting;
            ItemList.CellFormatting += ItemList_CellFormatting;

            // Do not auto-select the first item on startup
            ClearGridSelection();

            // Disable searchTextBox autocomplete/dropdown (designer textbox named searchTextBox)
            if (searchTextBox != null)
            {
                // Turn off suggestion dropdown — user types plain text and we filter programmatically
                searchTextBox.AutoCompleteMode = AutoCompleteMode.None;
                searchTextBox.AutoCompleteSource = AutoCompleteSource.None;
                // ensure handler wired
                searchTextBox.TextChanged -= SearchTextBox_TextChanged;
                searchTextBox.TextChanged += SearchTextBox_TextChanged;
            }

            // Ensure edit checkbox default: unchecked -> editing disabled
            if (editCheckBox != null)
            {
                editCheckBox.CheckedChanged -= editCheckBox_CheckedChanged;
                editCheckBox.Checked = false;
                editCheckBox.CheckedChanged += editCheckBox_CheckedChanged;
                // Apply initial mode
                SetEditMode(editCheckBox.Checked);
            }

            // Wire vente fields events (safety: UI might be null in some tests)
            try
            {
                if (txtVenteNbrLitres != null)
                {
                    txtVenteNbrLitres.TextChanged -= VenteFields_TextChanged;
                    txtVenteNbrLitres.TextChanged += VenteFields_TextChanged;
                }

                if (txtVentePrix != null)
                {
                    txtVentePrix.TextChanged -= VenteFields_TextChanged;
                    txtVentePrix.TextChanged += VenteFields_TextChanged;
                }
            }
            catch
            {
                // ignore wiring errors
            }

            // Load data automatically on startup
            RefreshUsers();

            // Populate parameters inputs from DB and vente defaults
            LoadParameters();
            // Update the window title using the CompanyName from Parameters
            UpdateWindowTitleFromParameters();
            // Attach live-update handler so title follows textbox changes
            try
            {
                if (txtCompanyName != null)
                {
                    txtCompanyName.TextChanged -= TxtCompanyName_TextChanged;
                    txtCompanyName.TextChanged += TxtCompanyName_TextChanged;
                }
            }
            catch { }
            PopulateVenteDefaultPrice();

            // Wire live recalculation for unit price / paid liters (if designer textboxes exist)
            try
            {
                if (textBox5 != null)
                {
                    textBox5.TextChanged -= PriceOrPaidLiters_TextChanged;
                    textBox5.TextChanged += PriceOrPaidLiters_TextChanged;
                }

                if (textBox6 != null)
                {
                    textBox6.TextChanged -= PriceOrPaidLiters_TextChanged;
                    textBox6.TextChanged += PriceOrPaidLiters_TextChanged;
                }

                // Ensure txtPortion validates/clamps on leaving the control
                if (txtPortion != null)
                {
                    txtPortion.Leave -= TxtPortion_Leave;
                    txtPortion.Leave += TxtPortion_Leave;

                    // live update when portion changes
                    txtPortion.TextChanged -= TxtPortion_TextChanged;
                    txtPortion.TextChanged += TxtPortion_TextChanged;
                }

                // Recalculate when the source quantity changes (textBox4 holds NbrLiters)
                if (textBox4 != null)
                {
                    textBox4.TextChanged -= TextBox4_TextChanged;
                    textBox4.TextChanged += TextBox4_TextChanged;
                }

                // Populate unit price input with default (if available and empty)
                PopulateDefaultUnitPrice();
            }
            catch
            {
                // Swallow UI wiring errors
            }

            try
            {
                ConfigureStatsGrid();

                // Populate lists and stats now that grids are configured
                RefreshUsers();
                LoadParameters();

                // Refresh the stats UI immediately
                BtnRefreshStats_Click(this, EventArgs.Empty);
            }
            catch
            {
                // swallow UI wiring errors
            }

            // Load today's ventes into the UI list
            try
            {
                // Populate vente year combo for the Vente tab
                if (venteYearComboBox != null)
                {
                    venteYearComboBox.SelectedIndexChanged -= VenteYearComboBox_SelectedIndexChanged;
                    venteYearComboBox.Items.Clear();
                    venteYearComboBox.Items.Add("Tous");

                    using var ctx = new DataContext();
                    var venteYears = ctx.Ventes?
                        .Select(v => v.CreatedAt.Year)
                        .Distinct()
                        .OrderByDescending(y => y)
                        .ToList();

                    if (venteYears != null && venteYears.Count > 0)
                    {
                        foreach (var y in venteYears) venteYearComboBox.Items.Add(y.ToString());
                        venteYearComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        var current = DateTime.Now.Year;
                        for (int i = 0; i < 5; i++) venteYearComboBox.Items.Add((current - i).ToString());
                        venteYearComboBox.SelectedIndex = 0;
                    }

                    venteYearComboBox.SelectedIndexChanged += VenteYearComboBox_SelectedIndexChanged;
                }

                LoadVentesToday();
            }
            catch
            {
                // ignore
            }

            // in Form1_Load (after RefreshUsers/LoadParameters or near UI wiring)
            try
            {
                if (yearComboBox != null)
                {
                    yearComboBox.SelectedIndexChanged -= YearComboBox_SelectedIndexChanged;
                    yearComboBox.Items.Clear();
                    yearComboBox.Items.Add("Tous");

                    // populate with distinct years present in DB (defensive)
                    using var ctx = new DataContext();
                    var years = ctx.Users?
                        .Where(u => u.CreatedAt.HasValue)
                        .Select(u => u.CreatedAt!.Value.Year)
                        .Distinct()
                        .OrderByDescending(y => y)
                        .ToList();

                    if (years != null && years.Count > 0)
                    {
                        foreach (var y in years) yearComboBox.Items.Add(y.ToString());
                        yearComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        // fallback: add last 5 years
                        var current = DateTime.Now.Year;
                        for (int i = 0; i < 5; i++) yearComboBox.Items.Add((current - i).ToString());
                        yearComboBox.SelectedIndex = 0;
                    }

                    yearComboBox.SelectedIndexChanged += YearComboBox_SelectedIndexChanged;
                }
            }
            catch
            {
                // ignore UI wiring errors
            }

            // Radio buttons wiring and default mode selection
            try
            {
                if (rdoPortion != null && rdoPaiement != null)
                {
                    rdoPortion.CheckedChanged -= rdoMode_CheckedChanged;
                    rdoPaiement.CheckedChanged -= rdoMode_CheckedChanged;

                    // default to Portion mode
                    rdoPortion.Checked = true;

                    rdoPortion.CheckedChanged += rdoMode_CheckedChanged;
                    rdoPaiement.CheckedChanged += rdoMode_CheckedChanged;

                    ApplyMode();
                }
            }
            catch
            {
                // ignore
            }
        }

        // ---------------------------
        // Utilities and helper methods
        // ---------------------------

        // Smart decimal formatter used across the form

        private static string FormatDecimalSmart(decimal value)
        {
            return decimal.Truncate(value) == value
                ? value.ToString("N0", CultureInfo.CurrentCulture)
                : value.ToString("N1", CultureInfo.CurrentCulture);
        }

        // Refresh the UI list of users from the database and bind to the BindingSource.

        private void RefreshUsers()
        {
            try
            {
                using var ctx = new DataContext();
                var users = ctx.Users?.OrderBy(u => u.Name).ToList() ?? new List<User>();
                DatabaseUsers = users;
                usersBinding.DataSource = DatabaseUsers;
                // Ensure grid doesn't auto-select first row
                ClearGridSelection();
            }
            catch
            {
                // swallow DB/UI errors
            }
        }

        // Load application Parameters into the UI controls (defensive)

        private void LoadParameters()
        {
            try
            {
                if (txtCompanyName == null && txtCompanyAddress == null && txtCompanyPhone == null && txtPricePerLiter == null && txtPortion == null)
                    return;

                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters != null)
                {
                    if (txtCompanyName != null) txtCompanyName.Text = parameters.CompanyName ?? string.Empty;
                    if (txtCompanyAddress != null) txtCompanyAddress.Text = parameters.CompanyAddress ?? string.Empty;
                    if (txtCompanyPhone != null) txtCompanyPhone.Text = parameters.CompanyPhone ?? string.Empty;
                    if (txtPricePerLiter != null && parameters.DefaultUnitPrice != 0m) txtPricePerLiter.Text = parameters.DefaultUnitPrice.ToString(CultureInfo.CurrentCulture);
                    if (txtPortion != null) txtPortion.Text = (parameters.DefaultPortion * 100m).ToString(CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                // swallow DB errors
            }
        }

        private void PopulateVenteDefaultPrice()
        {
            try
            {
                if (txtVentePrix == null) return;
                if (!string.IsNullOrWhiteSpace(txtVentePrix.Text)) return;

                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters == null) return;

                if (parameters.DefaultUnitPrice != 0m)
                {
                    txtVentePrix.Text = parameters.DefaultUnitPrice.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                // swallow
            }
        }

        // Populate textBox5 with default unit price (if empty) from Parameters or UI fallback

        private void PopulateDefaultUnitPrice()
        {
            try
            {
                if (textBox5 == null) return;
                if (!string.IsNullOrWhiteSpace(textBox5.Text)) return;

                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters != null && parameters.DefaultUnitPrice != 0m)
                {
                    textBox5.Text = parameters.DefaultUnitPrice.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                // swallow
            }
        }

        private void ConfigureStatsGrid()
        {
            try
            {
                if (dgvStats != null) dgvStats.AutoGenerateColumns = false;
                if (dgvVenteStats != null) dgvVenteStats.AutoGenerateColumns = false;
            }
            catch
            {
                // swallow
            }
        }

        // Apply a simple text filter to the in-memory users list and rebind

        private void ApplyFilter(string text)
        {
            try
            {
                if (DatabaseUsers == null || usersBinding == null)
                    return;

                if (string.IsNullOrWhiteSpace(text))
                {
                    usersBinding.DataSource = DatabaseUsers;
                    ClearGridSelection();
                    return;
                }

                var term = text.Trim();
                var filtered = DatabaseUsers.Where(u =>
                    (!string.IsNullOrEmpty(u.Name) && u.Name.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    || (!string.IsNullOrEmpty(u.Phone) && u.Phone.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    || (!string.IsNullOrEmpty(u.Address) && u.Address.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0)
                ).ToList();

                usersBinding.DataSource = filtered;
                ClearGridSelection();
            }
            catch
            {
                // swallow
            }
        }

        // Numbering/config for ItemList

        private void ConfigureGridColumns()
        {
            if (ItemList == null) return;

            ItemList.AutoGenerateColumns = false;
            // Make header text bold for ItemList
            try
            {
                var headerFont = new Font(ItemList.Font, FontStyle.Bold);
                ItemList.ColumnHeadersDefaultCellStyle.Font = headerFont;
            }
            catch
            {
                // ignore font errors
            }

            // Keep existing columns (designer already added the N° column). Clear other runtime columns to avoid duplicates.
            var numberCol = ItemList.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Name == "colNumber");
            ItemList.Columns.Clear();
            if (numberCol != null) ItemList.Columns.Add(numberCol);

            // Example columns for User entity; adjust as needed for your model
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "Nom",
                DataPropertyName = "Name",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPhone",
                HeaderText = "Téléphone",
                DataPropertyName = "Phone",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colAddress",
                HeaderText = "Adresse",
                DataPropertyName = "Address",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Removed Rendement column per request: do not add 'colRendement' to the grid
        }

        private void ItemList_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Number visible rows in the "N°" column (if present)
            try
            {
                if (ItemList != null && ItemList.Columns.Contains("colNumber"))
                {
                    for (int i =0; i < ItemList.Rows.Count; i++)
                    {
                        var row = ItemList.Rows[i];
                        // Only set numbering for non-new rows
                        if (!row.IsNewRow)
                            row.Cells["colNumber"].Value = (i +1).ToString(CultureInfo.CurrentCulture);
                    }
                }

                // Do NOT mutate cell.Value here. CellFormatting will handle presentation without
                // changing the underlying bound data, which avoids re-entrant binding events.
            }
            catch
            {
                // ignore numbering errors
            }

            // Optionally, clear selection after data binding to avoid auto-selecting the first row
            ClearGridSelection();
        }

        // Use CellFormatting to present the Rendement column without mutating the underlying value
        private void ItemList_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (ItemList == null) return;
                if (e.ColumnIndex <0 || e.ColumnIndex >= ItemList.Columns.Count) return;

                var col = ItemList.Columns[e.ColumnIndex];
                if (!string.Equals(col.Name, "colRendement", StringComparison.InvariantCultureIgnoreCase))
                    return;

                var raw = e.Value?.ToString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    e.Value = string.Empty;
                    e.FormattingApplied = true;
                    return;
                }

                // Avoid double-prefixing if underlying value already includes the label
                if (raw.StartsWith("Rendement:", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Ensure unit text exists
                    if (!raw.EndsWith("L/Q", StringComparison.InvariantCultureIgnoreCase))
                        e.Value = raw + " L/Q";
                    else
                        e.Value = raw;
                }
                else
                {
                    e.Value = $"{raw}";
                }

                e.FormattingApplied = true;
            }
            catch
            {
                // swallow per-row formatting errors
            }
        }

        // Load today's ventes into the DataGridView (now supports year filter and pagination)

        private void LoadVentesToday()
        {
            try
            {
                if (dgvVentesToday == null) return;

                using var ctx = new DataContext();
                // Build base query (read-only)
                var query = (ctx.Ventes ?? Enumerable.Empty<Vente>().AsQueryable()).AsNoTracking();

                // Apply year filter from venteYearComboBox if present
                try
                {
                    if (venteYearComboBox != null && venteYearComboBox.SelectedItem != null)
                    {
                        var sel = venteYearComboBox.SelectedItem.ToString();
                        if (!string.IsNullOrEmpty(sel) && !sel.Equals("Tous", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (int.TryParse(sel, out int selYear))
                            {
                                query = query.Where(v => v.CreatedAt.Year == selYear);
                            }
                        }
                    }
                }
                catch
                {
                    // swallow filtering errors
                }

                // compute totals for pagination (server-side)
                var totalCount = query.Count();
                venteTotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)ventePageSize));
                if (venteCurrentPage < 1) venteCurrentPage = 1;
                if (venteCurrentPage > venteTotalPages) venteCurrentPage = venteTotalPages;

                // fetch paged items
                var ventes = query
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((venteCurrentPage - 1) * ventePageSize)
                    .Take(ventePageSize)
                    .ToList();

                dgvVentesToday.Rows.Clear();
                var fr = CultureInfo.GetCultureInfo("fr-FR");

                // Correct date format: hours:minutes day-month-year
                const string dateFormat = "HH:mm dd-MM-yyyy";

                foreach (var v in ventes)
                {
                    var formattedDate = v.CreatedAt.ToString(dateFormat, fr);
                    dgvVentesToday.Rows.Add(v.Id, formattedDate, v.NbrLitres.ToString("N0", fr), v.Prix.ToString("N2", fr), v.Montant.ToString("N2", fr));
                }

                // Update pagination UI (include total items)
                if (lblVentePage != null)
                {
                    lblVentePage.Text = $"Page {venteCurrentPage} / {venteTotalPages} — {totalCount} Ventes";
                }

                if (btnVentePrev != null) btnVentePrev.Enabled = venteCurrentPage > 1;
                if (btnVenteNext != null) btnVenteNext.Enabled = venteCurrentPage < venteTotalPages;
            }
            catch
            {
                // ignore errors populating view
            }
        }

        // Event handler for vente fields to update montant live

        private void VenteFields_TextChanged(object? sender, EventArgs e)
        {
            UpdateVenteMontant();
        }

        private void UpdateVenteMontant()
        {
            try
            {
                if (lblVenteMontantValue == null) return;

                if (!TryParseInt(txtVenteNbrLitres.Text, out int? litersNullable))
                {
                    lblVenteMontantValue.Text = string.Empty;
                    return;
                }
                if (!TryParseDecimal(txtVentePrix.Text, out decimal price))
                {
                    lblVenteMontantValue.Text = string.Empty;
                    return;
                }

                var liters = litersNullable ?? 0;
                var montant = liters * price;

                lblVenteMontantValue.Text = montant != 0m ? FormatDecimalSmart(montant) : string.Empty;
            }
            catch
            {
                // ignore
            }
        }

        // Parsing helpers

        private static bool TryParseDecimal(string? s, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                   || decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
        }

        private static bool TryParseInt(string? s, out int? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out int v) ||
                int.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out v))
            {
                value = v;
                return true;
            }
            return false;
        }

        private static bool TryParseDecimal(string? s, out decimal? value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(s)) return false;
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v) ||
                decimal.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out v))
            {
                value = v;
                return true;
            }
            return false;
        }

        // ---------------------------
        // Event handler implementations referenced by Designer
        // ---------------------------

        private void editCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                if (editCheckBox != null)
                    SetEditMode(editCheckBox.Checked);
            }
            catch
            {
                // swallow UI errors
            }
        }

        private void SetEditMode(bool enabled)
        {
            try
            {
                // Buttons
                if (createBtn != null) createBtn.Enabled = enabled;
                if (updateBtn != null) updateBtn.Enabled = enabled;
                if (deleteBtn != null) deleteBtn.Enabled = enabled;

                // All TextBox inputs across tabs (defensive null checks)
                var textBoxes = new TextBox[]
                {
                        nameTextBox, addressTextBox,
                        textBox1, textBox2, textBox3, textBox4, textBox5, textBox6, textBox7,
                        weightTextBox,
                        txtCompanyName, txtCompanyAddress, txtCompanyPhone, txtPricePerLiter, txtPortion
                };

                foreach (var tb in textBoxes)
                {
                    if (tb != null) tb.Enabled = enabled;
                }

                // Ensure search box is always enabled
                if (searchTextBox != null) searchTextBox.Enabled = true;

                // Parameter save button
                if (btnSaveParameters != null) btnSaveParameters.Enabled = enabled;

                // Radio buttons: keep them disabled until user enables edit mode
                if (rdoPortion != null) rdoPortion.Enabled = enabled;
                if (rdoPaiement != null) rdoPaiement.Enabled = enabled;

                // New behavior:
                // - When edit mode is enabled (editCheckBox.Checked == true) the payment fields
                //   textBox5/6/7 must be disabled and Portion mode selected.
                if (enabled)
                {
                    try
                    {
                        if (textBox5 != null) textBox5.Enabled = false;
                        if (textBox6 != null) textBox6.Enabled = false;
                        if (textBox7 != null) textBox7.Enabled = false;
                    }
                    catch { }

                    try
                    {
                        if (rdoPortion != null) rdoPortion.Checked = true;
                        // Ensure UI reflects the selected mode (clears payment fields, etc.)
                        ApplyMode();
                    }
                    catch { }
                }
                else
                {
                    // When edit mode is disabled, keep payment fields disabled as before
                    try
                    {
                        if (textBox5 != null) textBox5.Enabled = false;
                        if (textBox6 != null) textBox6.Enabled = false;
                        if (textBox7 != null) textBox7.Enabled = false;
                    }
                    catch { }
                }
            }
            catch
            {
                // ignore UI errors
            }
        }

        private void rdoMode_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                ApplyMode();
            }
            catch
            {
                // swallow
            }
        }

        // Apply selected mode:
        // - Portion: clear and disable textBox5, textBox6, textBox7
        // - Paiement: enable textBox5, textBox6, textBox7; populate unit price & portion-liters from Parameters (or UI fallbacks)

        private void ApplyMode()
        {
            try
            {
                if (textBox5 == null || textBox6 == null || textBox7 == null || textBox4 == null)
                    return;

                // When radio buttons are not present or not enabled, do nothing
                if ((rdoPortion == null || rdoPaiement == null) || (!rdoPortion.Enabled && !rdoPaiement.Enabled))
                    return;

                bool isPortion = rdoPortion != null && rdoPortion.Checked;

                if (isPortion)
                {
                    // Clear and disable payment fields
                    textBox5.Text = string.Empty;
                    textBox6.Text = string.Empty;
                    textBox7.Text = string.Empty;

                    textBox5.Enabled = false;
                    textBox6.Enabled = false;
                    textBox7.Enabled = false;
                }
                else
                {
                    // Paiement mode: enable fields
                    textBox5.Enabled = true;
                    textBox6.Enabled = true;
                    textBox7.Enabled = true;

                    decimal unitPrice = 0m;
                    decimal portionFraction = 0m; // stored as 0..1

                    try
                    {
                        using var ctx = new DataContext();
                        var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                        if (parameters != null)
                        {
                            unitPrice = parameters.DefaultUnitPrice;
                            portionFraction = parameters.DefaultPortion;
                        }
                    }
                    catch
                    {
                        // ignore DB errors
                    }

                    // fallbacks to UI controls if DB didn't provide values
                    if (unitPrice == 0m && txtPricePerLiter != null && TryParseDecimal(txtPricePerLiter.Text, out decimal tmpPrice))
                        unitPrice = tmpPrice;

                    if (portionFraction == 0m && txtPortion != null && TryParseDecimal(txtPortion.Text, out decimal tmpPortionPercent))
                        portionFraction = tmpPortionPercent / 100m;

                    // compute portion liters from textBox4 (source liters)
                    int sourceLiters = 0;
                    if (TryParseInt(textBox4.Text, out int? srcNullable))
                        sourceLiters = srcNullable ?? 0;

                    var portionDecimal = sourceLiters * portionFraction;

                    // Show decimals for portion (do not round to int)
                    if (portionDecimal != 0m)
                        textBox6.Text = FormatDecimalSmart(portionDecimal);
                    else
                        textBox6.Text = string.Empty;

                    if (unitPrice != 0m)
                        textBox5.Text = unitPrice.ToString(CultureInfo.CurrentCulture);

                    // Recalculate total using existing shared method
                    PriceOrPaidLiters_TextChanged(this, EventArgs.Empty);
                }
            }
            catch
            {
                // swallow UI errors
            }
        }

        // Compute textBox6 = textBox4 * portion (txtPortion as percent 0..100).
        // Writes a decimal into textBox6 using current culture (no integer rounding).

        private void ApplyPortionToTextBox6()
        {
            try
            {
                if (suppressPortionApply) return;
                if (textBox4 == null || textBox6 == null || txtPortion == null) return;

                // Parse source liters (textBox4) as integer (the source number of liters is typically integer)
                if (!TryParseInt(textBox4.Text, out int? litersNullable))
                {
                    // if source is not numeric, clear target to avoid stale values
                    textBox6.Text = string.Empty;
                    return;
                }

                var liters = litersNullable ?? 0;

                // Parse portion as percent (0..100)
                if (!TryParseDecimal(txtPortion.Text, out decimal portionPercent))
                {
                    // invalid portion -> do not update
                    return;
                }

                // Clamp portion to [0,100]
                if (portionPercent < 0m) portionPercent = 0m;
                if (portionPercent > 100m) portionPercent = 100m;

                var fraction = portionPercent / 100m;
                var adjustedDecimal = liters * fraction;

                // Prevent re-entrancy while updating the target textbox
                suppressPortionApply = true;
                try
                {
                    // Show decimal values (smart formatting: N0 when integer, N1 when fractional)
                    textBox6.Text = adjustedDecimal != 0m ? FormatDecimalSmart(adjustedDecimal) : string.Empty;
                }
                finally
                {
                    suppressPortionApply = false;
                }
            }
            catch
            {
                // swallow UI errors
            }
        }

        private void TxtPortion_Leave(object? sender, EventArgs e)
        {
            try
            {
                if (txtPortion == null) return;

                if (!TryParseDecimal(txtPortion.Text, out decimal portionPercent))
                {
                    txtPortion.Text = string.Empty;
                    return;
                }

                if (portionPercent < 0m) portionPercent = 0m;
                if (portionPercent > 100m) portionPercent = 100m;

                txtPortion.Text = portionPercent.ToString(CultureInfo.CurrentCulture);

                // Update dependent fields
                ApplyPortionToTextBox6();
            }
            catch
            {
                // swallow UI errors
            }
        }

        private void TxtPortion_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                ApplyPortionToTextBox6();
            }
            catch
            {
                // swallow
            }
        }

        private void TextBox4_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                // When source liters change, recompute portion & mode-dependent values
                ApplyPortionToTextBox6();
                ApplyMode();
            }
            catch
            {
                // swallow
            }
        }

        // Recalculate AmountDue = UnitPrice * PayedLiters and update textBox7 live.
        // Accept decimal values for textBox6 (payed liters) so totals use fractional liters.

        private void PriceOrPaidLiters_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                if (textBox5 == null || textBox6 == null || textBox7 == null) return;

                if (!TryParseDecimal(textBox5.Text, out decimal unitPrice))
                {
                    // if unit price unparsable, clear amount (avoid misleading 0)
                    textBox7.Text = string.Empty;
                    return;
                }

                // allow decimal liters
                if (!TryParseDecimal(textBox6.Text, out decimal payedLitersDecimal))
                {
                    textBox7.Text = string.Empty;
                    return;
                }

                var amount = payedLitersDecimal * unitPrice;

                textBox7.Text = amount != 0m ? FormatDecimalSmart(amount) : string.Empty;
            }
            catch
            {
                // swallow UI errors
            }
        }

        // Designer-referenced no-op or minimal handlers

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void label7_Click(object sender, EventArgs e)
        {
        }

        // Fixed signatures: provide identifier name for second parameter

        private void label8_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
        }

        private void Poids_Click(object sender, EventArgs e)
        {
        }

        private void weightTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshUsers();
                ClearFormFields();
            }
            catch
            {
                // swallow
            }
        }

        private void lblCompanyAddressPhone_Click(object sender, EventArgs e)
        {
        }

        private void txtCompanyAddressPhone_TextChanged(object sender, EventArgs e)
        {
        }

        private void lblPortion_Click(object sender, EventArgs e)
        {
        }

        private void BtnSaveParameters_Click(object sender, EventArgs e)
        {
            try
            {
                // Defensive: nothing to save if controls are missing
                if (txtCompanyName == null && txtCompanyAddress == null && txtCompanyPhone == null && txtPricePerLiter == null && txtPortion == null)
                    return;

                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters == null)
                {
                    // No parameters row found — inform the user (optional: create a row automatically instead)
                    MessageBox.Show("Aucun enregistrement de paramètres trouvé dans la base de données. Créez d'abord un enregistrement (Id=1) ou initialisez la table.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Read text fields (defensive null checks)
                parameters.CompanyName = txtCompanyName?.Text.Trim() ?? string.Empty;
                parameters.CompanyAddress = txtCompanyAddress?.Text.Trim() ?? string.Empty;
                parameters.CompanyPhone = txtCompanyPhone?.Text.Trim() ?? string.Empty;

                // Parse price per liter
                if (txtPricePerLiter != null && TryParseDecimal(txtPricePerLiter.Text, out decimal parsedPrice))
                    parameters.DefaultUnitPrice = parsedPrice;
                else
                    parameters.DefaultUnitPrice = 0m;

                // Parse portion percent (0..100) and convert to fraction (0..1)
                decimal portionPercent = 0m;
                if (txtPortion != null && TryParseDecimal(txtPortion.Text, out decimal parsedPortion))
                    portionPercent = parsedPortion;

                if (portionPercent < 0m) portionPercent = 0m;
                if (portionPercent > 100m) portionPercent = 100m;
                parameters.DefaultPortion = portionPercent / 100m;

                // Persist changes
                ctx.SaveChanges();

                // Refresh UI and dependent values
                LoadParameters();
                PopulateVenteDefaultPrice();
                PopulateDefaultUnitPrice();
                ApplyMode();

                // Update title now that company name may have changed
                UpdateWindowTitleFromParameters();
                try
                {
                    if (txtCompanyName != null)
                    {
                        txtCompanyName.TextChanged -= TxtCompanyName_TextChanged;
                        txtCompanyName.TextChanged += TxtCompanyName_TextChanged;
                    }
                }
                catch { }

                MessageBox.Show("Paramètres sauvegardés et rechargés.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ParametersCheckBox.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement des paramètres : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ParametersCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (ParametersCheckBox != null)
                {
                    bool enabled = ParametersCheckBox.Checked;
                    if (txtCompanyName != null) txtCompanyName.Enabled = enabled;
                    if (txtCompanyAddress != null) txtCompanyAddress.Enabled = enabled;
                    if (txtCompanyPhone != null) txtCompanyPhone.Enabled = enabled;
                    if (txtPricePerLiter != null) txtPricePerLiter.Enabled = enabled;
                    if (txtPortion != null) txtPortion.Enabled = enabled;
                    if (btnSaveParameters != null) btnSaveParameters.Enabled = enabled;
                }
            }
            catch
            {
                // swallow
            }
        }

        private void btnEnregistrerVente_Click(object sender, EventArgs e)
        {
            try
            {
                // Parse inputs
                if (!TryParseInt(txtVenteNbrLitres?.Text, out int? nbrLitresNullable) || nbrLitresNullable == null)
                {
                    MessageBox.Show("Nombre de litres invalide.");
                    return;
                }

                if (!TryParseDecimal(txtVentePrix?.Text, out decimal prix))
                {
                    MessageBox.Show("Prix invalide.");
                    return;
                }

                var vente = new Vente
                {
                    NbrLitres = nbrLitresNullable.Value,
                    Prix = prix,
                    CreatedAt = DateTime.Now
                    // Montant will be computed by DataContext.UpdateDisplayFields before SaveChanges
                };

                // Persist
                using (var ctx = new DataContext())
                {
                    if (ctx.Ventes == null)
                    {
                        MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Ventes' est nul.");
                        return;
                    }

                    ctx.Ventes.Add(vente);
                    ctx.SaveChanges();
                }

                // Reset to first page and Refresh UI
                venteCurrentPage = 1;
                LoadVentesToday();



                // Use EscPosPrinter helper to print the vente receipt (delegates to QuestPdfPrinter.GeneratePdfAndOpenVente today).
                // Only attempt printing when the "print receipt" checkbox is present and checked.
                if (chkPrintReceipt != null && chkPrintReceipt.Checked)
                {
                    try
                    {
                        // Pass null to printerName to use default behavior; adjust if you want a specific printer name.
                        EscPosPrinter.PrintVenteReceipt(printerName: null, vente: vente);
                        MessageBox.Show("Vente enregistrée et impression demandée.");
                    }
                    catch (Exception ex)
                    {
                        // Keep save successful even if print fails
                        MessageBox.Show("Vente enregistrée. Erreur lors de l'impression : " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Vente enregistrée.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'enregistrement de la vente : " + ex.Message);
            }
        }

        // Implement delete button click on today's ventes grid

        private void DgvVentesToday_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                var dgv = sender as DataGridView ?? dgvVentesToday;
                if (dgv == null) return;

                var column = dgv.Columns[e.ColumnIndex];
                if (column == null) return;

                // Detect a "delete" button click robustly:
                bool isDeleteButton =
                    column is DataGridViewButtonColumn
                    || column.CellType == typeof(DataGridViewButtonCell)
                    || string.Equals(column.Name, "colVenteDelete", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(column.HeaderText, "Supprimer", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(column.HeaderText, "Delete", StringComparison.InvariantCultureIgnoreCase);

                if (!isDeleteButton) return;

                var row = dgv.Rows[e.RowIndex];
                if (row == null) return;

                // Try to find the id cell first by expected name, then fall back to scanning row values.
                object? idObj = null;
                if (dgv.Columns.Contains("colVenteId"))
                {
                    idObj = row.Cells["colVenteId"].Value;
                }
                else
                {
                    // fallback: find the first cell containing an int-like value
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell?.Value == null) continue;
                        if (int.TryParse(cell.Value.ToString(), out _))
                        {
                            idObj = cell.Value;
                            break;
                        }
                    }
                }

                if (idObj == null || !int.TryParse(idObj.ToString(), out int venteId)) return;

                var confirm = MessageBox.Show("Supprimer cette vente ?", "Confirmer la suppression", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                using var ctx = new DataContext();
                if (ctx.Ventes == null)
                {
                    MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Ventes' est nul.");
                    return;
                }

                var vente = ctx.Ventes.Find(venteId);
                if (vente == null)
                {
                    MessageBox.Show("Vente introuvable.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                ctx.Ventes.Remove(vente);
                ctx.SaveChanges();

                // Refresh UI after deletion: keep current page if possible, else move back a page
                using var ctx2 = new DataContext();
                var total = ctx2.Ventes?.Count() ?? 0;
                var newTotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)ventePageSize));
                if (venteCurrentPage > newTotalPages) venteCurrentPage = newTotalPages;

                LoadVentesToday();
                BtnRefreshStats_Click(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression de la vente : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // sync combo selection with radio buttons if needed
                if (modeComboBox != null)
                {
                    if (modeComboBox.SelectedItem?.ToString() == "Portion")
                    {
                        if (rdoPortion != null) rdoPortion.Checked = true;
                    }
                    else
                    {
                        if (rdoPaiement != null) rdoPaiement.Checked = true;
                    }
                }
                ApplyMode();
            }
            catch
            {
                // swallow
            }
        }

        // Pagination controls for ventes

        private void BtnVentePrev_Click(object? sender, EventArgs e)
        {
            try
            {
                if (venteCurrentPage > 1)
                {
                    venteCurrentPage--;
                    LoadVentesToday();
                }
            }
            catch { }
        }

        private void BtnVenteNext_Click(object? sender, EventArgs e)
        {
            try
            {
                if (venteCurrentPage < venteTotalPages)
                {
                    venteCurrentPage++;
                    LoadVentesToday();
                }
            }
            catch { }
        }

        // Year selection changed for vente tab

        private void VenteYearComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                // Reset to page 1 and reload ventes for the selected year
                venteCurrentPage = 1;
                LoadVentesToday();
            }
            catch { }
        }

        // Called when user changes selection

        private void ItemList_SelectedRowsChanged(object? sender, EventArgs e)
        {
            if (suppressSelectionEvents) return;

            if (!ItemList.Focused)
            {
                ClearFormFields();
                return;
            }

            if (ItemList.SelectedRows == null || ItemList.SelectedRows.Count == 0)
            {
                ClearFormFields();
                return;
            }

            var selectedRow = ItemList.SelectedRows[0];
            var selectedUser = selectedRow.DataBoundItem as User;
            if (selectedUser is not null)
            {
                nameTextBox.Text = selectedUser.Name;
                textBox1.Text = selectedUser.Phone;
                addressTextBox.Text = selectedUser.Address;
                textBox2.Text = selectedUser.NbrBags != 0m ? FormatDecimalSmart(selectedUser.NbrBags) : string.Empty;
                textBox3.Text = selectedUser.NbrContainers;
                textBox4.Text = selectedUser.NbrLiters?.ToString() ?? string.Empty;

                decimal effectiveUnitPrice = 0m;
                bool haveUnitPrice = false;

                if (selectedUser.UnitPriceLiter.HasValue && selectedUser.UnitPriceLiter.Value != 0m)
                {
                    effectiveUnitPrice = selectedUser.UnitPriceLiter.Value;
                    textBox5.Text = FormatDecimalSmart(effectiveUnitPrice);
                    haveUnitPrice = true;
                }
                else
                {
                    try
                    {
                        using var ctx = new DataContext();
                        if (ctx.Parameters != null)
                        {
                            var parameters = ctx.Parameters.FirstOrDefault(p => p.Id == 1);
                            if (parameters != null && parameters.DefaultUnitPrice != 0m)
                            {
                                effectiveUnitPrice = parameters.DefaultUnitPrice;
                                textBox5.Text = FormatDecimalSmart(effectiveUnitPrice);
                                haveUnitPrice = true;
                            }
                            else
                            {
                                textBox5.Text = string.Empty;
                            }
                        }
                        else
                        {
                            textBox5.Text = string.Empty;
                        }
                    }
                    catch
                    {
                        textBox5.Text = string.Empty;
                    }
                }

                textBox6.Text = selectedUser.PayedLiters?.ToString() ?? string.Empty;

                if (selectedUser.AmountDue.HasValue && selectedUser.AmountDue.Value != 0m)
                {
                    textBox7.Text = FormatDecimalSmart(selectedUser.AmountDue.Value);
                }
                else if (haveUnitPrice && (selectedUser.PayedLiters ?? 0) != 0)
                {
                    var amount = (selectedUser.PayedLiters ?? 0) * effectiveUnitPrice;
                    textBox7.Text = amount != 0m ? FormatDecimalSmart(amount) : string.Empty;
                }
                else
                {
                    textBox7.Text = string.Empty;
                }

                weightTextBox.Text = selectedUser.Weight.HasValue && selectedUser.Weight.Value != 0m
                    ? FormatDecimalSmart(selectedUser.Weight.Value)
                    : string.Empty;

                // NEW: set rendement display control if present (defensive)
                try
                {
                    string? dispRend = null;
                    if (!string.IsNullOrWhiteSpace(selectedUser.DisplayRendement))
                    {
                        // Show only the value + unit in the input textbox (no "Rendement:" label)
                        dispRend = $"{selectedUser.DisplayRendement} L/Q";
                    }
                    else if (selectedUser.Weight.HasValue && selectedUser.Weight.Value !=0m && selectedUser.NbrLiters.HasValue && selectedUser.NbrLiters.Value !=0)
                    {
                        // compute fallback when persisted display not available
                        var litres = (decimal)selectedUser.NbrLiters.Value;
                        var poids = selectedUser.Weight.Value;
                        var rendement = (litres *100m) / poids;
                        // Only show the formatted value plus unit
                        dispRend = $"{FormatDecimalSmart(rendement)} L/Q";
                    }

                    if (dispRend != null)
                    {
                        // defensive: if a TextBox named rendementTextBox exists in the designer, fill it
                        try { if (rendementTextBox != null) rendementTextBox.Text = dispRend; } catch { }
                    }
                    else
                    {
                        try { if (rendementTextBox != null) rendementTextBox.Text = string.Empty; } catch { }
                    }
                }
                catch
                {
                    // ignore
                }

                // Restore persisted Portion / Paiement mode for this user
                try
                {
                    if (rdoPortion != null && rdoPaiement != null)
                    {
                        // Temporarily detach handlers to avoid duplicate ApplyMode calls
                        rdoPortion.CheckedChanged -= rdoMode_CheckedChanged;
                        rdoPaiement.CheckedChanged -= rdoMode_CheckedChanged;

                        bool isPortion = selectedUser.IsPortionMode ?? true; // default to Portion
                        rdoPortion.Checked = isPortion;
                        rdoPaiement.Checked = !isPortion;

                        // Reattach handlers and ensure UI consistent
                        rdoPortion.CheckedChanged += rdoMode_CheckedChanged;
                        rdoPaiement.CheckedChanged += rdoMode_CheckedChanged;

                        ApplyMode();
                    }
                }
                catch
                {
                    // ignore radio restore errors
                }
            }
            else
            {
                ClearFormFields();
            }
        }

        // Utility: clear form fields

        private void ClearFormFields()
        {
            try
            {
                if (nameTextBox != null) nameTextBox.Text = string.Empty;
                if (textBox1 != null) textBox1.Text = string.Empty;
                if (addressTextBox != null) addressTextBox.Text = string.Empty;
                if (textBox2 != null) textBox2.Text = string.Empty;
                if (textBox3 != null) textBox3.Text = string.Empty;
                if (textBox4 != null) textBox4.Text = string.Empty;
                if (textBox5 != null) textBox5.Text = string.Empty;
                if (textBox6 != null) textBox6.Text = string.Empty;
                if (textBox7 != null) textBox7.Text = string.Empty;
                if (weightTextBox != null) weightTextBox.Text = string.Empty;

                if (txtCompanyName != null) txtCompanyName.Text = string.Empty;
                if (txtCompanyAddress != null) txtCompanyAddress.Text = string.Empty;
                if (txtCompanyPhone != null) txtCompanyPhone.Text = string.Empty;
                if (txtPricePerLiter != null) txtPricePerLiter.Text = string.Empty;
                if (txtPortion != null) txtPortion.Text = string.Empty;

                if (txtVenteNbrLitres != null) txtVenteNbrLitres.Text = string.Empty;
                if (txtVentePrix != null) txtVentePrix.Text = string.Empty;

                // NEW: clear rendement control if present
                try { if (rendementTextBox != null) rendementTextBox.Text = string.Empty; } catch { }
            }
            catch
            {
                // swallow
            }
        }

        private void ClearGridSelection()
        {
            suppressSelectionEvents = true;
            try
            {
                usersBinding.Position = -1;
                ItemList.ClearSelection();
                try { ItemList.CurrentCell = null; } catch { }
            }
            finally
            {
                suppressSelectionEvents = false;
            }
        }

        // Search box handler wired in Initialize / Designer

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                ApplyFilter(searchTextBox?.Text ?? string.Empty);
            }
            catch
            {
                // swallow UI errors
            }
        }

        // Refresh statistics — wired from Designer and called from Form1_Load

        private void BtnRefreshStats_Click(object? sender, EventArgs e)
        {
            try
            {
                using var context = new DataContext();
                var users = context.Users?.ToList() ?? new List<User>();

                // Load ventes
                var ventes = context.Ventes?.ToList() ?? new List<Vente>();

                // Apply year filter if selected
                try
                {
                    if (yearComboBox != null && yearComboBox.SelectedItem != null)
                    {
                        var sel = yearComboBox.SelectedItem.ToString();
                        if (!string.IsNullOrEmpty(sel) && !sel.Equals("Tous", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (int.TryParse(sel, out int selYear))
                            {
                                users = users.Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value.Year == selYear).ToList();
                                ventes = ventes.Where(v => v.CreatedAt.Year == selYear).ToList();
                            }
                        }
                    }
                }
                catch
                {
                    // swallow filtering errors – fallback to unfiltered list
                }

                var totalClients = users.Count;
                var totalSacs = users.Sum(u => u.NbrBags);
                var totalPoids = users.Sum(u => u.Weight ?? 0m);

                decimal totalLitresProduites = users.Sum(u => (decimal)(u.NbrLiters ?? 0));
                decimal totalLitresVendues = users.Sum(u => (decimal)(u.PayedLiters ?? 0));

                var totalRevenueFromPaidLiters = users.Sum(u => (u.PayedLiters ?? 0) * (u.UnitPriceLiter ?? 0m));
                var totalAmountDue = users.Sum(u => u.AmountDue ?? 0m);

                var totalVentesLitres = ventes.Sum(v => v.NbrLitres);
                var totalVentesRecette = ventes.Sum(v => v.Montant);

                decimal portionFraction = 0m;
                try
                {
                    var parameters = context.Parameters?.FirstOrDefault(p => p.Id == 1);
                    if (parameters != null) portionFraction = parameters.DefaultPortion;
                }
                catch
                {
                    portionFraction = 0m;
                }

                decimal totalPortionEntrees = 0m;
                decimal totalPortionVendues = 0m;

                foreach (var u in users)
                {
                    var liters = (decimal)(u.NbrLiters ?? 0);
                    if (liters == 0m) continue;

                    var userPortion = liters * portionFraction;

                    if (u.AmountDue.HasValue && u.AmountDue.Value != 0m)
                        totalPortionVendues += userPortion;
                    else
                        totalPortionEntrees += userPortion;
                }

                decimal totalNombreLitresLivrees = totalLitresProduites - totalPortionEntrees;

                var ci = CultureInfo.GetCultureInfo("fr-FR");

#if DEBUG   
                Debug.WriteLine($"[Stats DBG] produced={totalLitresProduites}, portionFraction={portionFraction}, portionEntrées={totalPortionEntrees}, portionVendues={totalPortionVendues}, delivered={totalNombreLitresLivrees}");
#endif

                if (dgvStats != null)
                {
                    dgvStats.Rows.Clear();

                    static string FormatDecimalSmartForLabel(decimal value, CultureInfo culture)
                    {
                        return decimal.Truncate(value) == value
                            ? value.ToString("N0", culture)
                            : value.ToString("N1", culture);
                    }

                    dgvStats.Rows.Add("Total Clients", totalClients.ToString("N0", ci));
                    dgvStats.Rows.Add("Sacs entrée", FormatDecimalSmartForLabel(totalSacs, ci));
                    dgvStats.Rows.Add("Poids", FormatDecimalSmartForLabel(totalPoids, ci));
                    dgvStats.Rows.Add("Litres produites", totalLitresProduites.ToString("N0", ci));
                    dgvStats.Rows.Add("Nombre Litres Portion vendues", totalPortionVendues.ToString("N1", ci));
                    dgvStats.Rows.Add("Total Nombre de litre Portion Entrées", totalPortionEntrees.ToString("N1", ci));
                    dgvStats.Rows.Add("Total Nombre de litre livrées", totalNombreLitresLivrees.ToString("N1", ci));
                    dgvStats.Rows.Add("Recette (litres vendues)", totalRevenueFromPaidLiters.ToString("N2", ci));

                    // NEW: Total huile vendu shown under main stats = Recette (litres vendues) + Ventes - Recette (journalisée)
                    var totalHuileVendueMonetary = totalRevenueFromPaidLiters + totalVentesRecette;
                    dgvStats.Rows.Add("Total huile vendu", totalHuileVendueMonetary.ToString("N2", ci));
                }

                if (dgvVenteStats != null)
                {
                    dgvVenteStats.Rows.Clear();
                    dgvVenteStats.Rows.Add("Total ventes (enregistrements)", ventes.Count.ToString("N0", ci));
                    dgvVenteStats.Rows.Add("Ventes - Litres (journalisées)", totalVentesLitres.ToString("N0", ci));
                    dgvVenteStats.Rows.Add("Ventes - Recette (journalisée)", totalVentesRecette.ToString("N2", ci));

                    // Note: 'Total huile vendu' is displayed in the main stats grid (dgvStats).
                }

                // Refresh today's ventes view
                LoadVentesToday();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec du chargement des statistiques : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Designer-wired click handlers (ItemList and CRUD buttons). Minimal but functional.
        // ItemList CellContentClick (wired in Designer)

        private void ItemList_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
        }

        // Create user button handler (wired in Designer)

        private void createBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                var name = nameTextBox.Text.Trim();
                var phone = textBox1.Text.Trim();
                var address = addressTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
                {
                    MessageBox.Show("Le nom et l'adresse sont requis.");
                    return;
                }

                if (!TryParseDecimal(textBox2.Text, out decimal nbrBags))
                    nbrBags = 0m;

                var nbrContainers = textBox3.Text.Trim();

                if (!TryParseInt(textBox4.Text, out int? nbrLitersNullable))
                    nbrLitersNullable = null;

                if (!TryParseDecimal(textBox5.Text, out decimal unitPrice))
                    unitPrice = 0m;

                if (!TryParseInt(textBox6.Text, out int? payedLitersNullable))
                    payedLitersNullable = null;

                if (!TryParseDecimal(weightTextBox.Text, out decimal? weightNullable))
                    weightNullable = null;

                decimal amountDue = 0m;
                if (TryParseDecimal(textBox7.Text, out decimal parsedAmount))
                    amountDue = parsedAmount;
                else
                    amountDue = (payedLitersNullable ?? 0) * unitPrice;

                using (var context = new DataContext())
                {
                    if (context.Users == null)
                    {
                        MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Users' est nul.");
                        return;
                    }

                    var user = new User
                    {
                        Name = name,
                        Phone = phone,
                        Address = address,
                        NbrBags = nbrBags,
                        NbrContainers = nbrContainers,
                        NbrLiters = nbrLitersNullable,
                        UnitPriceLiter = unitPrice,
                        PayedLiters = payedLitersNullable,
                        AmountDue = amountDue,
                        Weight = weightNullable,
                        CreatedAt = DateTime.Now,
                        // Persist selected mode
                        IsPortionMode = rdoPortion?.Checked ?? true
                    };

                    context.Users.Add(user);
                    context.SaveChanges();
                }

                MessageBox.Show("Utilisateur créé avec succès");
                RefreshUsers();
                if (editCheckBox != null) editCheckBox.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la création: " + ex.Message);
            }
        }

        // Update selected user (wired in Designer)

        private void updateBtn_Click(object? sender, EventArgs e)
        {
            if (ItemList.CurrentRow == null)
            {
                MessageBox.Show("Aucun utilisateur sélectionné.");
                return;
            }

            var selectedUser = ItemList.CurrentRow.DataBoundItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Aucun utilisateur sélectionné.");
                return;
            }

            var name = nameTextBox.Text.Trim();
            var phone = textBox1.Text.Trim();
            var address = addressTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address))
            {
                MessageBox.Show("Le nom et l'adresse sont requis.");
                return;
            }

            if (!TryParseDecimal(textBox2.Text, out decimal nbrBags))
                nbrBags = 0m;

            var nbrContainers = textBox3.Text.Trim();

            if (!TryParseInt(textBox4.Text, out int? nbrLitersNullable))
                nbrLitersNullable = null;

            if (!TryParseDecimal(textBox5.Text, out decimal unitPrice))
                unitPrice = 0m;

            if (!TryParseInt(textBox6.Text, out int? payedLitersNullable))
                payedLitersNullable = null;

            if (!TryParseDecimal(textBox7.Text, out decimal amountDue))
                amountDue = (payedLitersNullable ?? 0) * unitPrice;

            if (!TryParseDecimal(weightTextBox.Text, out decimal? weightNullable))
                weightNullable = null;

            try
            {
                using var context = new DataContext();
                if (context.Users == null)
                {
                    MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Users' est nul.");
                    return;
                }

                var userToUpdate = context.Users.Find(selectedUser.Id);
                if (userToUpdate != null)
                {
                    userToUpdate.Name = name;
                    userToUpdate.Phone = phone;
                    userToUpdate.Address = address;
                    userToUpdate.NbrBags = nbrBags;
                    userToUpdate.NbrContainers = nbrContainers;
                    userToUpdate.NbrLiters = nbrLitersNullable;
                    userToUpdate.UnitPriceLiter = unitPrice;
                    userToUpdate.PayedLiters = payedLitersNullable;
                    userToUpdate.AmountDue = amountDue;
                    userToUpdate.Weight = weightNullable;

                    // Persist selected mode
                    userToUpdate.IsPortionMode = rdoPortion?.Checked ?? true;

                    context.SaveChanges();
                    MessageBox.Show("Utilisateur mis à jour avec succès");
                    if (editCheckBox != null) editCheckBox.Checked = false;
                }
                else
                {
                    MessageBox.Show("L'utilisateur sélectionné est introuvable dans la base de données.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la mise à jour : " + ex.Message);
            }

            RefreshUsers();
        }

        // Delete selected user (wired in Designer)

        private void deleteBtn_Click(object? sender, EventArgs e)
        {
            if (ItemList.CurrentRow == null)
            {
                MessageBox.Show("Aucun utilisateur sélectionné.");
                return;
            }

            var selectedUser = ItemList.CurrentRow.DataBoundItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("Aucun utilisateur sélectionné.");
                return;
            }

            try
            {
                using var context = new DataContext();
                if (context.Users == null)
                {
                    MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Users' est nul.");
                    return;
                }

                var user = context.Users.Find(selectedUser.Id);
                if (user == null)
                {
                    MessageBox.Show("Utilisateur introuvable.");
                    return;
                }

                context.Users.Remove(user);
                context.SaveChanges();
                MessageBox.Show("Utilisateur supprimé avec succès");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la suppression : " + ex.Message);
            }

            RefreshUsers();
            if (editCheckBox != null) editCheckBox.Checked = false;
        }

        // Tab control selected index changed (wired in Designer)

        private void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (mainTabControl.SelectedTab == tabParameters)
                    LoadParameters();

                if (mainTabControl.SelectedTab == tabVente)
                {
                    PopulateVenteDefaultPrice();
                    // keep current page and selected year; reload ventes
                    LoadVentesToday();
                }
            }
            catch
            {
                // swallow
            }
        }

        // Added handler for yearComboBox SelectedIndexChanged (wired in Designer)

        private void YearComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                // Refresh statistics when the selected year changes
                BtnRefreshStats_Click(this, EventArgs.Empty);
            }
            catch
            {
                // swallow UI errors
            }
        }
        // Designer-wired print button handler (was missing)


        private void printBtn_Click(object sender, EventArgs e)
        {
            if (ItemList.CurrentRow == null)
            {
                MessageBox.Show("Sélectionnez un utilisateur à imprimer.");
                return;
            }

            var selectedUser = ItemList.CurrentRow.DataBoundItem as User;
            if (selectedUser == null)
            {
                MessageBox.Show("La ligne sélectionnée n'est pas un utilisateur.");
                return;
            }

            try
            {
                // Create PDF bytes with existing QuestPdfPrinter helper
                var pdfBytes = QuestPdfPrinter.CreatePdfBytes(selectedUser);

                // Try to print silently (uses SumatraPDF if installed). If printing fails,
                // fall back to opening the PDF so the user can print manually.
                if (TryPrintPdfSilently(pdfBytes, printerName: null))
                {
                    MessageBox.Show("Tâche d'impression envoyée.");
                }
                else
                {
                    // fallback to the existing preview/open behaviour
                    QuestPdfPrinter.GeneratePdfAndOpen(selectedUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec de l'impression : " + ex.Message);
            }
        }

        private bool TryPrintPdfSilently(byte[] pdfBytes, string? printerName)
        {
            if (pdfBytes == null || pdfBytes.Length == 0) return false;
            var tmp = Path.Combine(Path.GetTempPath(), $"user_ticket_{Guid.NewGuid():N}.pdf");
            File.WriteAllBytes(tmp, pdfBytes);

            try
            {
                // 1) Prefer SumatraPDF (supports silent printing). Check common install locations.
                string? sumatra = GetSumatraPath();
                if (!string.IsNullOrEmpty(sumatra) && File.Exists(sumatra))
                {
                    // -print-to-default or -print-to "Printer Name" (if printerName supplied)
                    var args = string.IsNullOrEmpty(printerName)
                        ? $"-print-to-default -silent \"{tmp}\""
                        : $"-print-to \"{printerName}\" -silent \"{tmp}\"";

                    var psi = new ProcessStartInfo(sumatra, args)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                    return true;
                }

                // 2) Fallback: ask the OS to print using the associated application.
                // Note: this may show UI depending on the associated application.
                var psiShell = new ProcessStartInfo(tmp)
                {
                    Verb = "Print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };

                Process.Start(psiShell);
                return true;
            }
            catch
            {
                // if printing failed, let caller fall back to preview/open
                return false;
            }
        }

        private static string? GetSumatraPath()
        {
            var candidates = new[]
            {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "SumatraPDF", "SumatraPDF.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SumatraPDF", "SumatraPDF.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SumatraPDF", "SumatraPDF.exe")
                };

            return candidates.FirstOrDefault(File.Exists);
        }

        // Update the Form's Text using the CompanyName stored in Parameters (Id =1)
        private void UpdateWindowTitleFromParameters()
        {
            try
            {
                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters != null && !string.IsNullOrWhiteSpace(parameters.CompanyName))
                {
                    this.Text = $"GESTION CLIENTS - {parameters.CompanyName}";
                }
                else
                {
                    this.Text = "GESTION CLIENTS";
                }
            }
            catch
            {
                // fallback to default title on error
                try { this.Text = "GESTION CLIENTS"; } catch { }
            }
        }

        // Live-update window title while editing company name in the Parameters tab
        private void TxtCompanyName_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                if (txtCompanyName == null) return;
                var name = txtCompanyName.Text?.Trim() ?? string.Empty;
                this.Text = string.IsNullOrEmpty(name) ? "GESTION CLIENTS" : $"GESTION CLIENTS - {name}";
            }
            catch
            {
                // ignore
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }
    }
}

