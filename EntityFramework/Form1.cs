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

        // Replaces previous bool suppressPortionApply;
        private bool suppressPortionApply;

        public List<User> DatabaseUsers { get; private set; } = new List<User>();

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
            ItemList.SelectionChanged += ItemList_SelectedRowsChanged;
            ItemList.DataBindingComplete -= ItemList_DataBindingComplete;
            ItemList.DataBindingComplete += ItemList_DataBindingComplete;

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
                    yearComboBox.Items.Add("All");

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
        }

        // Populate vente price from Parameters.DefaultUnitPrice when empty
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

        // Load today's ventes into the DataGridView
        private void LoadVentesToday()
        {
            try
            {
                if (dgvVentesToday == null) return;

                using var ctx = new DataContext();
                var today = DateTime.Today;
                var ventes = ctx.Ventes?
                    .Where(v => v.CreatedAt.Date == today)
                    .OrderByDescending(v => v.CreatedAt)
                    .ToList() ?? new List<Vente>();

                dgvVentesToday.Rows.Clear();
                var fr = CultureInfo.GetCultureInfo("fr-FR");
                foreach (var v in ventes)
                {
                    var time = v.CreatedAt.ToString("HH:mm", fr);
                    dgvVentesToday.Rows.Add(v.Id, time, v.NbrLitres.ToString("N0", fr), v.Prix.ToString("N2", fr), v.Montant.ToString("N2", fr));
                }
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

        private void BtnRefreshStats_Click(object sender, EventArgs e)
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
                        if (!string.IsNullOrEmpty(sel) && !sel.Equals("All", StringComparison.CurrentCultureIgnoreCase))
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
                var totalLitresProduites = users.Sum(u => u.NbrLiters ?? 0);   // total produced liters (int)
                var totalLitresVendues = users.Sum(u => u.PayedLiters ?? 0);   // paid/sold liters (int)

                var totalRevenueFromPaidLiters = users.Sum(u => (u.PayedLiters ?? 0) * (u.UnitPriceLiter ?? 0m));
                var totalAmountDue = users.Sum(u => u.AmountDue ?? 0m);

                // Ventes aggregates
                var totalVentesLitres = ventes.Sum(v => v.NbrLitres);
                var totalVentesRecette = ventes.Sum(v => v.Montant);

                // Get portion fraction (stored as 0..1). If missing, remains 0.
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

                // Compute total portion from aggregate produced liters, then round
                var totalPortionLiters = (int)Math.Round(totalLitresProduites * portionFraction, MidpointRounding.AwayFromZero);

                // Total delivered = produced - portion
                var totalNombreLitresLivrees = totalLitresProduites - totalPortionLiters;

                var ci = CultureInfo.GetCultureInfo("fr-FR");

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

                    // Paid/sold liters (as before)
                    dgvStats.Rows.Add("Nombre Litres vendues", totalLitresVendues.ToString("N0", ci));

                    // Vente-specific stats
                    dgvStats.Rows.Add("Ventes - Litres (journalisées)", totalVentesLitres.ToString("N0", ci));
                    dgvStats.Rows.Add("Ventes - Recette (journalisée)", totalVentesRecette.ToString("N2", ci));

                    // New rows per your request
                    dgvStats.Rows.Add("Total Nombre de litre Portion", totalPortionLiters.ToString("N0", ci));
                    dgvStats.Rows.Add("Total Nombre de litre livrées", totalNombreLitresLivrees.ToString("N0", ci));

                    dgvStats.Rows.Add("Recette (litres vendues)", totalRevenueFromPaidLiters.ToString("N2", ci));
                    dgvStats.Rows.Add("Total dû (clients)", totalAmountDue.ToString("N2", ci));
                    // "Moyenne dû" intentionally removed
                }

                // Refresh today's ventes view
                LoadVentesToday();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec du chargement des statistiques : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When user navigates to the Parameters tab, reload values from DB (keeps UI in sync)
            try
            {
                if (mainTabControl.SelectedTab == tabParameters)
                    LoadParameters();

                if (mainTabControl.SelectedTab == tabVente)
                {
                    PopulateVenteDefaultPrice();
                    LoadVentesToday();
                }
            }
            catch
            {
                // swallow — LoadParameters already shows errors if needed
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void updateBtn_Click(object sender, EventArgs e)
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

            // Read values from form
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

            // Parse weight (nullable)
            if (!TryParseDecimal(weightTextBox.Text, out decimal? weightNullable))
                weightNullable = null;

            using (var context = new DataContext())
            {
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

                    context.SaveChanges();
                    MessageBox.Show("Utilisateur mis à jour avec succès");
                    editCheckBox.Checked = false;
                }
                else
                {
                    MessageBox.Show("L'utilisateur sélectionné est introuvable dans la base de données.");
                }
            }

            // Refresh list
            RefreshUsers();
        }

        private void createBtn_Click(object sender, EventArgs e)
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

            // Parse weight (nullable)
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
                };

                context.Users.Add(user);
                context.SaveChanges();
            }

            MessageBox.Show("Utilisateur créé avec succès");

            // Refresh list
            RefreshUsers();
            editCheckBox.Checked = false;
        }

        private void readBtn_Click(object sender, EventArgs e)
        {
            // kept for designer wiring - forward to central refresh
            RefreshUsers();
        }

        private void deleteBtn_Click(object sender, EventArgs e)
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

            using (var context = new DataContext())
            {
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
            }

            MessageBox.Show("Utilisateur supprimé avec succès");
            // Refresh the list
            RefreshUsers();
            editCheckBox.Checked = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void ItemList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // Called when user changes selection (wired in Form1_Load)
        // Populate inputs only when the DataGridView currently has focus.
        // Programmatic selection changes (during binding/filtering) will not focus the grid,
        // so inputs remain empty until the user explicitly focuses/selects a row.

        private void ItemList_SelectedRowsChanged(object? sender, EventArgs e)
        {
            if (suppressSelectionEvents) return;

            // If the grid isn't focused, do not populate inputs (this avoids auto-population during binding)
            if (!ItemList.Focused)
            {
                ClearFormFields();
                return;
            }

            // Only populate when a row is actively selected (prevents current-cell-only cases)
            if (ItemList.SelectedRows == null || ItemList.SelectedRows.Count == 0)
            {
                ClearFormFields();
                return;
            }

            var selectedRow = ItemList.SelectedRows[0];
            var selectedUser = selectedRow.DataBoundItem as User;
            if (selectedUser is not null)
            {
                // Map user properties to form fields
                nameTextBox.Text = selectedUser.Name;
                textBox1.Text = selectedUser.Phone;
                addressTextBox.Text = selectedUser.Address;

                // NbrBags is decimal (required) — format smartly or clear when zero
                textBox2.Text = selectedUser.NbrBags != 0m ? FormatDecimalSmart(selectedUser.NbrBags) : string.Empty;

                textBox3.Text = selectedUser.NbrContainers;
                textBox4.Text = selectedUser.NbrLiters?.ToString() ?? string.Empty;

                // Unit price: prefer value from user; if missing/zero, populate from Parameters (single-row id=1)
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
                        // swallow DB errors and leave unit price empty
                        textBox5.Text = string.Empty;
                    }
                }

                textBox6.Text = selectedUser.PayedLiters?.ToString() ?? string.Empty;

                // AmountDue: if stored use it; otherwise compute from effectiveUnitPrice * PayedLiters when possible
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
            }
            else
            {
                ClearFormFields();
            }
        }

        // Print button click (now uses QuestPDF only)

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

        // Helper: try to print PDF bytes without showing a preview.
        // Returns true when a print process was started, false when not.

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

        // Try a few typical SumatraPDF locations

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

        // Utility: clear form fields

        private void ClearFormFields()
        {
            nameTextBox.Text = string.Empty;
            textBox1.Text = string.Empty;
            addressTextBox.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            textBox4.Text = string.Empty;
            textBox5.Text = string.Empty;
            textBox6.Text = string.Empty;
            textBox7.Text = string.Empty;
            weightTextBox.Text = string.Empty;

            // Company parameter fields
            txtCompanyName.Text = string.Empty;
            txtCompanyAddress.Text = string.Empty;
            txtCompanyPhone.Text = string.Empty;
            txtPricePerLiter.Text = string.Empty;
            txtPortion.Text = string.Empty;

            // Vente fields
            txtVenteNbrLitres.Text = string.Empty;
            txtVentePrix.Text = string.Empty;
        }

        // Ensure grid / binding source has no selection and do not fire selection handler while doing it

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

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
        }

        private void label6_Click(object sender, EventArgs e)
        {
        }

        private void label8_Click(object sender, EventArgs e)
        {
        }

        // Add this method to Form1 to fix CS0103

        private void ConfigureGridColumns()
        {
            if (ItemList == null) return;

            ItemList.AutoGenerateColumns = false;
            ItemList.Columns.Clear();

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

            // Add more columns as needed for your User properties
        }

        // Add this method to Form1 to fix CS0103

        private void ItemList_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Optionally, clear selection after data binding to avoid auto-selecting the first row
            ClearGridSelection();
        }

        // Add this method to Form1 to fix CS0103

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Apply filter to users list based on search text
            ApplyFilter(searchTextBox?.Text ?? string.Empty);
        }

        // ---- New helper implementations to resolve missing references ----

        // Called when editCheckBox changes; wires up SetEditMode

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

        // Enable/disable create/update/delete buttons (minimal implementation)

        private void SetEditMode(bool enabled)
        {
            try
            {
                // Buttons
                if (createBtn != null) createBtn.Enabled = enabled;
                if (updateBtn != null) updateBtn.Enabled = enabled;
                if (deleteBtn != null) deleteBtn.Enabled = enabled;

                // All TextBox inputs across tabs (defensive null checks)
                // NOTE: intentionally exclude searchTextBox so it remains usable at all times.
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
            }
            catch
            {
                // ignore UI errors
            }
        }

        // Simple filter implementation: filter by name or phone containing the search text (case-insensitive)

        private void ApplyFilter(string filter)
        {
            try
            {
                var normalized = (filter ?? string.Empty).Trim();
                IEnumerable<User> list = DatabaseUsers ?? new List<User>();

                if (!string.IsNullOrEmpty(normalized))
                {
                    list = list.Where(u =>
                        (!string.IsNullOrEmpty(u.Name) && u.Name.IndexOf(normalized, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        || (!string.IsNullOrEmpty(u.Phone) && u.Phone.IndexOf(normalized, StringComparison.CurrentCultureIgnoreCase) >= 0));
                }

                // Update binding source - use a concrete list to avoid deferred execution issues
                usersBinding.DataSource = list.ToList();

                // Update autocomplete source if desired (kept disabled by default but method remains)
                UpdateAutoCompleteSource();

                // After applying filter, clear selection so inputs don't auto-populate
                ClearGridSelection();
            }
            catch
            {
                // ignore filter errors for robustness
            }
        }

        // Small utility to format decimals "smartly" similar to other formatters used in the file

        private static string FormatDecimalSmart(decimal value)
        {
            var culture = CultureInfo.CurrentCulture;
            return decimal.Truncate(value) == value
                ? value.ToString("N0", culture)
                : value.ToString("N1", culture);
        }

        // Designer-referenced event stubs (no-op or minimal) to stop CS0103 from Designer wires

        private void label3_Click(object? sender, EventArgs e)
        {
        }

        private void Poids_Click(object? sender, EventArgs e)
        {
        }

        private void weightTextBox_TextChanged(object? sender, EventArgs e)
        {
        }

        private void label9_Click(object? sender, EventArgs e)
        {
        }

        private void clearBtn_Click(object? sender, EventArgs e)
        {
            // Clear form fields and selection
            ClearFormFields();
            ClearGridSelection();
        }

        private void BtnSaveParameters_Click(object? sender, EventArgs e)
        {
            try
            {
                // Validate txtPortion first: empty => 0, otherwise must parse and be within [0, 100]
                decimal portionPercent = 0m;
                if (!string.IsNullOrWhiteSpace(txtPortion?.Text))
                {
                    if (!TryParseDecimal(txtPortion.Text, out decimal tmpPortion))
                    {
                        MessageBox.Show("La portion doit être un nombre valide (utilisez le format numérique de votre culture).", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPortion?.Focus();
                        return;
                    }

                    if (tmpPortion < 0m || tmpPortion > 100m)
                    {
                        MessageBox.Show("La portion doit être comprise entre 0 et 100 (pourcentage).", "Valeur hors plage", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPortion?.Focus();
                        return;
                    }

                    portionPercent = tmpPortion;
                }

                using var context = new DataContext();

                // Convert percent (0..100) to stored fraction (0..1)
                var storedPortion = portionPercent / 100m;

                if (context.Parameters == null)
                {
                    var p = new Parameters
                    {
                        CompanyName = txtCompanyName?.Text ?? string.Empty,
                        CompanyAddress = txtCompanyAddress?.Text ?? string.Empty,
                        CompanyPhone = txtCompanyPhone?.Text ?? string.Empty,
                        DefaultUnitPrice = TryParseDecimal(txtPricePerLiter?.Text, out decimal tmpPrice) ? tmpPrice : 0m,
                        DefaultPortion = storedPortion
                    };
                    context.Parameters?.Add(p);
                }
                else
                {
                    var parameters = context.Parameters.FirstOrDefault(p => p.Id == 1) ?? new Parameters();
                    parameters.CompanyName = txtCompanyName?.Text ?? string.Empty;
                    parameters.CompanyAddress = txtCompanyAddress?.Text ?? string.Empty;
                    parameters.CompanyPhone = txtCompanyPhone?.Text ?? string.Empty;
                    if (TryParseDecimal(txtPricePerLiter?.Text, out decimal price)) parameters.DefaultUnitPrice = price;

                    parameters.DefaultPortion = storedPortion;

                    if (parameters.Id == 0)
                        context.Parameters.Add(parameters);
                }

                context.SaveChanges();

                // Refresh UI (including the window title) from persisted parameters
                LoadParameters();

                MessageBox.Show("Paramètres sauvegardés.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ParametersCheckBox.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec de la sauvegarde des paramètres : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
        }

        // Called when the ParametersCheckBox in the Parameters tab is toggled.
        // Enables/disables parameter inputs and the save button; when enabling, reloads values.

        private void ParametersCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool enabled;
                if (sender is CheckBox cb)
                    enabled = cb.Checked;
                else
                    enabled = ParametersCheckBox?.Checked ?? false;

                if (txtCompanyName != null) txtCompanyName.Enabled = enabled;
                if (txtCompanyAddress != null) txtCompanyAddress.Enabled = enabled;
                if (txtCompanyPhone != null) txtCompanyPhone.Enabled = enabled;
                if (txtPricePerLiter != null) txtPricePerLiter.Enabled = enabled;
                if (txtPortion != null) txtPortion.Enabled = enabled;
                if (btnSaveParameters != null) btnSaveParameters.Enabled = enabled;

                if (enabled)
                {
                    // Refresh values from DB when entering edit mode
                    LoadParameters();
                    txtCompanyName?.Focus();
                }
            }
            catch
            {
                // Swallow UI errors to avoid unexpected crashes
            }
        }

        // -----------------------------------------------------------------

        // Populate textBox5 (unit price) from Parameters table when the field is empty.
        // This helps pre-fill the unit price for new user entries.

        private void PopulateDefaultUnitPrice()
        {
            try
            {
                if (textBox5 == null) return;

                // Only populate when empty to avoid overwriting user input or selected user values
                if (!string.IsNullOrWhiteSpace(textBox5.Text)) return;

                using var context = new DataContext();
                if (context.Parameters == null) return;

                var parameters = context.Parameters.FirstOrDefault(p => p.Id == 1);
                if (parameters == null) return;

                if (parameters.DefaultUnitPrice != 0m)
                {
                    textBox5.Text = parameters.DefaultUnitPrice.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch
            {
                // ignore DB errors here — this is a convenience helper
            }
        }

        // Recalculate AmountDue = UnitPrice * PayedLiters and update textBox7 live.

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

                if (!TryParseInt(textBox6.Text, out int? payedLitersNullable))
                {
                    textBox7.Text = string.Empty;
                    return;
                }

                var payed = payedLitersNullable ?? 0;
                var amount = payed * unitPrice;

                textBox7.Text = amount != 0m ? FormatDecimalSmart(amount) : string.Empty;
            }
            catch
            {
                // swallow UI errors
            }
        }

        private void lblCompanyAddressPhone_Click(object sender, EventArgs e)
        {
        }

        private void txtCompanyAddressPhone_TextChanged(object sender, EventArgs e)
        {
        }

        // New handler: clamp/format portion when leaving the field
        private void TxtPortion_Leave(object? sender, EventArgs e)
        {
            try
            {
                if (txtPortion == null) return;
                var text = txtPortion.Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    // empty is treated as 0%
                    txtPortion.Text = 0m.ToString(CultureInfo.CurrentCulture);
                    return;
                }

                if (!TryParseDecimal(text, out decimal parsed))
                {
                    MessageBox.Show("La portion doit être un nombre (format courant).", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPortion.Focus();
                    return;
                }

                // Clamp to [0,100] (percentage) and reformat using current culture
                if (parsed < 0m) parsed = 0m;
                if (parsed > 100m) parsed = 100m;

                txtPortion.Text = parsed.ToString(CultureInfo.CurrentCulture);
            }
            catch
            {
                // ignore UI errors
            }
        }

        private void lblPortion_Click(object sender, EventArgs e)
        {

        }

        // New handlers and helper: place these among the other private helpers in Form1

        private void TxtPortion_TextChanged(object? sender, EventArgs e)
        {
            ApplyPortionToTextBox6();
        }

        private void TextBox4_TextChanged(object? sender, EventArgs e)
        {
            ApplyPortionToTextBox6();
        }

        // Compute textBox6 = textBox4 * portion (txtPortion as percent 0..100).
        // Writes a rounded integer into textBox6 using current culture.
        private void ApplyPortionToTextBox6()
        {
            try
            {
                if (suppressPortionApply) return;
                if (textBox4 == null || textBox6 == null || txtPortion == null) return;

                // Parse source liters (textBox4)
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
                var adjustedInt = (int)Math.Round(adjustedDecimal, MidpointRounding.AwayFromZero);

                // Prevent re-entrancy while updating the target textbox
                suppressPortionApply = true;
                try
                {
                    textBox6.Text = adjustedInt.ToString(CultureInfo.CurrentCulture);
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

        private void ConfigureStatsGrid()
        {
            if (dgvStats == null) return;

            dgvStats.AutoGenerateColumns = false;
            dgvStats.Columns.Clear();
            dgvStats.RowHeadersVisible = false;
            dgvStats.AllowUserToAddRows = false;
            dgvStats.ReadOnly = true;
            dgvStats.EnableHeadersVisualStyles = false;

            dgvStats.ColumnHeadersDefaultCellStyle.Font = new Font(dgvStats.Font ?? SystemFonts.DefaultFont, FontStyle.Bold);

            dgvStats.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStat",
                HeaderText = "Statistique",
                DataPropertyName = null, // we add rows manually
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvStats.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colValue",
                HeaderText = "Valeur",
                DataPropertyName = null,
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
        }

        private void YearComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // reuse existing refresh method
            BtnRefreshStats_Click(this, EventArgs.Empty);
        }

        // New: handler to save a Vente record
        private void btnEnregistrerVente_Click(object? sender, EventArgs e)
        {
            try
            {
                if (txtVenteNbrLitres == null || txtVentePrix == null)
                {
                    MessageBox.Show("Les champs de vente sont introuvables.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!int.TryParse(txtVenteNbrLitres.Text?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out int nbrLitres))
                {
                    MessageBox.Show("Le nombre de litres doit être un entier valide.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtVenteNbrLitres.Focus();
                    return;
                }

                if (!TryParseDecimal(txtVentePrix.Text?.Trim(), out decimal prix))
                {
                    MessageBox.Show("Le prix doit être un nombre valide.", "Valeur invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtVentePrix.Focus();
                    return;
                }

                var vente = new Vente
                {
                    NbrLitres = nbrLitres,
                    Prix = prix,
                    Montant = nbrLitres * prix,
                    CreatedAt = DateTime.Now
                };

                using (var ctx = new DataContext())
                {
                    if (ctx.Ventes == null)
                    {
                        MessageBox.Show("Le contexte de la base de données n'est pas correctement configuré. Le DbSet 'Ventes' est nul.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ctx.Ventes.Add(vente);
                    ctx.SaveChanges();
                }

                // Clear vente inputs, focus, refresh stats and show a small toast
                txtVenteNbrLitres.Text = string.Empty;
                txtVentePrix.Text = string.Empty;
                txtVenteNbrLitres.Focus();

                // Refresh stats and today's ventes list
                BtnRefreshStats_Click(this, EventArgs.Empty);
                LoadVentesToday();

                ShowToast("Vente enregistrée avec succès");

                // If print checkbox checked, print via EscPosPrinter
                try
                {
                    if (chkPrintReceipt != null && chkPrintReceipt.Checked)
                    {
                        EscPosPrinter.PrintVenteReceipt(null, vente);
                    }
                }
                catch (Exception ex)
                {
                    // do not let printing failure block the save flow; show an informational message
                    MessageBox.Show("Échec de l'impression du reçu : " + ex.Message, "Impression échouée", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec de l'enregistrement de la vente : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Show a transient non-blocking toast message on the Vente tab
        private void ShowToast(string message, int milliseconds = 2000)
        {
            try
            {
                if (lblVenteToast == null || toastTimer == null) return;

                lblVenteToast.Text = message;
                lblVenteToast.Visible = true;
                lblVenteToast.BringToFront();

                toastTimer.Interval = milliseconds;
                toastTimer.Stop();
                toastTimer.Start();
            }
            catch
            {
                // ignore UI toast errors; fallback will be MessageBox if needed
            }
        }

        // Timer tick: hide the toast
        private void ToastTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (toastTimer != null) toastTimer.Stop();
                if (lblVenteToast != null) lblVenteToast.Visible = false;
            }
            catch
            {
                // ignore
            }
        }

        // Load parameters from DB into UI controls (fixes CS0103)
        private void LoadParameters()
        {
            try
            {
                using var ctx = new DataContext();
                var parameters = ctx.Parameters?.FirstOrDefault(p => p.Id == 1);
                if (parameters != null)
                {
                    if (txtCompanyName != null) txtCompanyName.Text = parameters.CompanyName ?? string.Empty;
                    if (txtCompanyAddress != null) txtCompanyAddress.Text = parameters.CompanyAddress ?? string.Empty;
                    if (txtCompanyPhone != null) txtCompanyPhone.Text = parameters.CompanyPhone ?? string.Empty;

                    if (txtPricePerLiter != null)
                    {
                        txtPricePerLiter.Text = parameters.DefaultUnitPrice != 0m
                            ? parameters.DefaultUnitPrice.ToString(CultureInfo.CurrentCulture)
                            : string.Empty;
                    }

                    if (txtPortion != null)
                    {
                        // stored as fraction (0..1) => show as percent (0..100)
                        txtPortion.Text = parameters.DefaultPortion != 0m
                            ? (parameters.DefaultPortion * 100m).ToString(CultureInfo.CurrentCulture)
                            : string.Empty;
                    }

                    // update window title or labels if required (optional)
                }
                else
                {
                    // ensure controls are cleared if no parameters present
                    if (txtCompanyName != null) txtCompanyName.Text = string.Empty;
                    if (txtCompanyAddress != null) txtCompanyAddress.Text = string.Empty;
                    if (txtCompanyPhone != null) txtCompanyPhone.Text = string.Empty;
                    if (txtPricePerLiter != null) txtPricePerLiter.Text = string.Empty;
                    if (txtPortion != null) txtPortion.Text = string.Empty;
                }
            }
            catch
            {
                // swallow load errors
            }
        }

        // Update autocomplete suggestions for searchTextBox (fixes CS0103)
        private void UpdateAutoCompleteSource()
        {
            try
            {
                if (searchTextBox == null) return;

                var names = (DatabaseUsers ?? new List<User>())
                    .Where(u => !string.IsNullOrWhiteSpace(u.Name))
                    .Select(u => u.Name)
                    .Distinct()
                    .ToArray();

                var ac = new AutoCompleteStringCollection();
                ac.AddRange(names);
                searchTextBox.AutoCompleteCustomSource = ac;
            }
            catch
            {
                // swallow
            }
        }
        // Add this method to handle the CellContentClick event for dgvVentesToday
        private void DgvVentesToday_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Example: Handle delete button click in the "Action" column
            if (e.RowIndex >= 0 && dgvVentesToday.Columns[e.ColumnIndex].Name == "colVenteDelete")
            {
                // You can add your delete logic here, e.g.:
                // var venteId = dgvVentesToday.Rows[e.RowIndex].Cells["colVenteId"].Value;
                // DeleteVenteById(venteId);
                // RefreshVentesGrid();
            }
        }
        // Add this method to Form1 to fix CS0103

        private void RefreshUsers()
        {
            try
            {
                using var context = new DataContext();
                if (context.Users == null)
                {
                    usersBinding.DataSource = new List<User>();
                    DatabaseUsers = new List<User>();
                    return;
                }

                // Load all users from the database
                var users = context.Users.ToList();
                DatabaseUsers = users;

                // Update the binding source
                usersBinding.DataSource = users;

                // After refreshing, clear selection so inputs don't auto-populate
                ClearGridSelection();
            }
            catch
            {
                // On error, clear the grid and binding source
                usersBinding.DataSource = new List<User>();
                DatabaseUsers = new List<User>();
            }
        }
    }
}
