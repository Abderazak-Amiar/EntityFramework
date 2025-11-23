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

            // Load data automatically on startup
            RefreshUsers();

            // Populate parameters inputs from DB
            LoadParameters();

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

                // Populate unit price input with default (if available and empty)
                PopulateDefaultUnitPrice();
            }
            catch
            {
                // Swallow UI wiring errors
            }
        }

        private void RefreshUsers()
        {
            try
            {
                using (var context = new DataContext())
                {
                    DatabaseUsers = context.Users?.ToList() ?? new List<User>();

                    // Autocomplete/dropdown disabled, so we no longer populate AutoCompleteCustomSource.
                    // Apply any active filter and refresh grid
                    ApplyFilter(searchTextBox?.Text ?? string.Empty);

                    // Ensure nothing is selected so input fields stay empty
                    ClearGridSelection();
                }
            }
            catch (Exception ex)
            {
                // Show a helpful message if the DB or schema is not available
                MessageBox.Show("Échec du chargement des utilisateurs : " + ex.Message, "Erreur de chargement", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateAutoCompleteSource()
        {
            if (searchTextBox == null) return;

            var names = DatabaseUsers
                .Where(u => !string.IsNullOrWhiteSpace(u.Name))
                .Select(u => u.Name.Trim())
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                // Order by name, then by phone number (secondary sort)
                .OrderBy(n => n)
                .ToArray();

            var source = new AutoCompleteStringCollection();
            source.AddRange(names);
            searchTextBox.AutoCompleteCustomSource = source;
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
                    Weight = weightNullable
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

        // Add this method to Form1 (near other private helpers)

        private void LoadParameters()
        {
            try
            {
                using var context = new DataContext();

                if (context.Parameters == null)
                {
                    // No parameters table available yet — clear inputs
                    txtCompanyName.Text = string.Empty;
                    txtCompanyAddress.Text = string.Empty;
                    txtCompanyPhone.Text = string.Empty;
                    txtPricePerLiter.Text = string.Empty;
                    txtPortion.Text = string.Empty;

                    // Update window title with placeholder when no table
                    SetWindowTitle(null);
                    return;
                }

                // Use single-row convention id = 1
                var parameters = context.Parameters.FirstOrDefault(p => p.Id == 1) ?? new Parameters();

                txtCompanyName.Text = parameters.CompanyName ?? string.Empty;
                txtCompanyAddress.Text = parameters.CompanyAddress ?? string.Empty;
                txtCompanyPhone.Text = parameters.CompanyPhone ?? string.Empty;

                // Show decimal values using current culture; TryParseDecimal already accepts both cultures on save.
                txtPricePerLiter.Text = parameters.DefaultUnitPrice != 0m
                    ? parameters.DefaultUnitPrice.ToString(System.Globalization.CultureInfo.CurrentCulture)
                    : string.Empty;

                txtPortion.Text = parameters.DefaultPortion != 0m
                    ? parameters.DefaultPortion.ToString(System.Globalization.CultureInfo.CurrentCulture)
                    : string.Empty;

                // Update the Form title to "GESTION CLIENTS - {CompanyName}".
                // When company name is empty, keep the literal "Company Name" as requested.
                SetWindowTitle(parameters.CompanyName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec du chargement des paramètres : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // New helper to centralize window title update
        // New helper to centralize window title update

        private void SetWindowTitle(string? companyName)
        {
            string display;
            if (string.IsNullOrWhiteSpace(companyName))
            {
                display = "COMPANY NAME";
            }
            else
            {
                var trimmed = companyName.Trim();
                try
                {
                    var ci = CultureInfo.CurrentCulture;
                    display = trimmed.ToUpper(ci);
                }
                catch
                {
                    // Fallback to invariant uppercase if culture-based conversion fails
                    display = trimmed.ToUpperInvariant();
                }
            }

            try
            {
                this.Text = $"GESTION CLIENTS - {display}";
            }
            catch
            {
                // ignore UI errors
            }
        }

        private void configureBtn_Click(object sender, EventArgs e)
        {
            // Switch to the Parameters tab in the new tab control.
            // Defensive null checks avoid crashes if designer wasn't fully initialized.
            try
            {
                if (mainTabControl != null && tabParameters != null)
                    mainTabControl.SelectedTab = tabParameters;
            }
            catch
            {
                // swallow — not critical; LoadParameters is called when tab is selected anyway
            }
        }

        // C#

        private void BtnRefreshStats_Click(object sender, EventArgs e)
        {
            try
            {
                // Query DB directly to ensure up-to-date numbers
                using var context = new DataContext();
                var users = context.Users?.ToList() ?? new List<User>();

                // Totals requested
                var totalClients = users.Count;
                var totalSacs = users.Sum(u => u.NbrBags);                     // Sacs entrée (decimal)
                var totalPoids = users.Sum(u => u.Weight ?? 0m);               // Poids (decimal)
                var totalLitresProduites = users.Sum(u => u.NbrLiters ?? 0);   // Litres produites (int)
                var totalLitresVendues = users.Sum(u => u.PayedLiters ?? 0);   // Nombre Litres vendues (int)

                // "Profit de litres vendues" interpreted as revenue from sold liters:
                // sum(payedLiters * unitPriceLiter) per user
                var totalRevenueFromPaidLiters = users.Sum(u => (u.PayedLiters ?? 0) * (u.UnitPriceLiter ?? 0m));

                // Keep existing debt-related stats for the UI
                var totalAmountDue = users.Sum(u => u.AmountDue ?? 0m);
                var averageDue = totalClients > 0 ? users.Average(u => (u.AmountDue ?? 0m)) : 0m;

                // Format and update UI
                var ci = CultureInfo.GetCultureInfo("fr-FR");

                // Ensure dgvStats exists
                if (dgvStats != null)
                {
                    dgvStats.Rows.Clear();

                    // Helper inline formatter to match existing "smart" formatting style
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
                    dgvStats.Rows.Add("Nombre Litres vendues", totalLitresVendues.ToString("N0", ci));
                    dgvStats.Rows.Add("Recette (litres vendues)", totalRevenueFromPaidLiters.ToString("N2", ci));
                    dgvStats.Rows.Add("Total dû (clients)", totalAmountDue.ToString("N2", ci));
                    dgvStats.Rows.Add("Moyenne dû", averageDue.ToString("N2", ci));
                }

                // Update the short summary label as before (keeps backward compatibility)
                lblStatsSummary.Text = $"Utilisateurs: {totalClients}, Total dû: {totalAmountDue.ToString("N2", ci)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Échec du chargement des statistiques : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Replace the placeholder MainTabControl_SelectedIndexChanged with this implementation

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When user navigates to the Parameters tab, reload values from DB (keeps UI in sync)
            try
            {
                if (mainTabControl.SelectedTab == tabParameters)
                    LoadParameters();
            }
            catch
            {
                // swallow — LoadParameters already shows errors if needed
            }
        }

        // Add this method to Form1 to fix CS0103

        private void ConfigureGridColumns()
        {
            // Clear any auto-generated columns
            ItemList.AutoGenerateColumns = false;
            ItemList.Columns.Clear();

            // Add columns for User properties (adjust as needed for your User model)
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Nom",
                Name = "colName",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Phone",
                HeaderText = "Téléphone",
                Name = "colPhone",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Address",
                HeaderText = "Adresse",
                Name = "colAddress",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NbrBags",
                HeaderText = "Sacs",
                Name = "colNbrBags",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NbrContainers",
                HeaderText = "Conteneurs",
                Name = "colNbrContainers",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NbrLiters",
                HeaderText = "Litres",
                Name = "colNbrLiters",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UnitPriceLiter",
                HeaderText = "Prix/Litre",
                Name = "colUnitPriceLiter",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "PayedLiters",
                HeaderText = "Litres Payés",
                Name = "colPayedLiters",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AmountDue",
                HeaderText = "Montant Dû",
                Name = "colAmountDue",
                ReadOnly = true
            });
            ItemList.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Weight",
                HeaderText = "Poids",
                Name = "colWeight",
                ReadOnly = true
            });
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

        private void label3_Click(object? sender, EventArgs e)         {
        }

        private void Poids_Click(object? sender, EventArgs e)         {
        }

        private void weightTextBox_TextChanged(object? sender, EventArgs e)         {
        }

        private void label9_Click(object? sender, EventArgs e)         {
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
                using var context = new DataContext();

                if (context.Parameters == null)
                {
                    // create new parameters row if table exists but no row
                    var p = new Parameters
                    {
                        CompanyName = txtCompanyName?.Text ?? string.Empty,
                        CompanyAddress = txtCompanyAddress?.Text ?? string.Empty,
                        CompanyPhone = txtCompanyPhone?.Text ?? string.Empty,
                        DefaultUnitPrice = TryParseDecimal(txtPricePerLiter?.Text, out decimal tmpPrice) ? tmpPrice : 0m,
                        DefaultPortion = TryParseDecimal(txtPortion?.Text, out decimal tmpPortion) ? tmpPortion : 0m
                    };
                    context.Parameters?.Add(p);
                }
                else
                {
                    // update single row convention id = 1
                    var parameters = context.Parameters.FirstOrDefault(p => p.Id == 1) ?? new Parameters();
                    parameters.CompanyName = txtCompanyName?.Text ?? string.Empty;
                    parameters.CompanyAddress = txtCompanyAddress?.Text ?? string.Empty;
                    parameters.CompanyPhone = txtCompanyPhone?.Text ?? string.Empty;
                    if (TryParseDecimal(txtPricePerLiter?.Text, out decimal price)) parameters.DefaultUnitPrice = price;
                    if (TryParseDecimal(txtPortion?.Text, out decimal portion)) parameters.DefaultPortion = portion;

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
    }
}
