namespace EntityFramework
{
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

                // Unit price, PayedLiters, AmountDue and Weight are nullable — format only when present/non-zero
                textBox5.Text = selectedUser.UnitPriceLiter.HasValue && selectedUser.UnitPriceLiter.Value != 0m
                    ? FormatDecimalSmart(selectedUser.UnitPriceLiter.Value)
                    : string.Empty;

                textBox6.Text = selectedUser.PayedLiters?.ToString() ?? string.Empty;

                textBox7.Text = selectedUser.AmountDue.HasValue && selectedUser.AmountDue.Value != 0m
                    ? FormatDecimalSmart(selectedUser.AmountDue.Value)
                    : string.Empty;

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

        // Add this method to the Form1 class (near other private helpers)

        private void ConfigureGridColumns()
        {
            // Do not auto-generate columns; we create them with French headers
            ItemList.AutoGenerateColumns = false;
            ItemList.Columns.Clear();
                
            // Ensure header style is applied: disable visual styles and set a bold header font
            ItemList.EnableHeadersVisualStyles = false;
            ItemList.ColumnHeadersDefaultCellStyle.Font = new Font(ItemList.Font, FontStyle.Bold);

            void AddColumn(string propertyName, string headerText, string? format = null)
            {
                var col = new DataGridViewTextBoxColumn
                {
                    DataPropertyName = propertyName,
                    Name = propertyName,
                    HeaderText = headerText,
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
                if (!string.IsNullOrEmpty(format))
                    col.DefaultCellStyle.Format = format;
                ItemList.Columns.Add(col);
            }

            AddColumn("Id", "N°");
            AddColumn("Name", "Nom");
            AddColumn("Phone", "Téléphone");
            AddColumn("Address", "Adresse");

            // Prefer display/formatted string columns so we control fraction display
            AddColumn("DisplayNbrBags", "Nbr Sacs");      // binds to User.DisplayNbrBags (string)
            AddColumn("NbrContainers", "Nbr Bidons");
            AddColumn("DisplayWeight", "Poids");          // binds to User.DisplayWeight (string)
            AddColumn("NbrLiters", "Litres");
            AddColumn("PayedLiters", "Litres payés");
            AddColumn("DisplayAmountDue", "Montant dû"); // binds to User.DisplayAmountDue (string)
        }

        private void Poids_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void weightTextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilter(searchTextBox?.Text ?? string.Empty);
        }

        private void ApplyFilter(string searchTerm)
        {
            // When changing DataSource we don't want intermediate selection changes to populate fields.
            suppressSelectionEvents = true;
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    usersBinding.DataSource = DatabaseUsers;
                }
                else
                {
                    var term = searchTerm.Trim();
                    var filtered = DatabaseUsers
                        // Match against Name, Phone, or Address
                        .Where(u => (!string.IsNullOrEmpty(u.Name) && u.Name.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(u.Phone) && u.Phone.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(u.Address) && u.Address.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0))
                        .ToList();
                    usersBinding.DataSource = filtered;
                }
            }
            finally
            {
                suppressSelectionEvents = false;
            }

            // keep no current item after filter so form inputs remain empty until user selects a row
            ClearGridSelection();
        }

        private void editCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            // If the control isn't present (designer not created yet) do nothing.
            if (editCheckBox == null) return;

            // Enable/disable form controls and buttons based on the checkbox state.
            SetEditMode(editCheckBox.Checked);
        }

        // Add this method to the Form1 class to fix CS0103

        private void SetEditMode(bool enabled)
        {
            // Example logic: enable/disable form fields based on edit mode
            nameTextBox.Enabled = enabled;
            textBox1.Enabled = enabled;
            addressTextBox.Enabled = enabled;
            textBox2.Enabled = enabled;
            textBox3.Enabled = enabled;
            textBox4.Enabled = enabled;
            textBox5.Enabled = enabled;
            textBox6.Enabled = enabled;
            textBox7.Enabled = enabled;
            weightTextBox.Enabled = enabled;

            // CRUD buttons enabled only in edit mode
            updateBtn.Enabled = enabled;
            createBtn.Enabled = enabled;
            deleteBtn.Enabled = enabled;

            // Print always available
            printBtn.Enabled = true;

            // Keep grid read-only to avoid inline edits; form controls handle edits
            ItemList.ReadOnly = true;
        }

        // Add this helper near other private helpers in Form1 class

        private static string FormatDecimalSmart(decimal value)
        {
            var ci = CultureInfo.CurrentCulture;
            return decimal.Truncate(value) == value
                ? value.ToString("N0", ci)   // no decimals when fractional part is zero
                : value.ToString("N1", ci);  // one decimal otherwise
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            // Clear all input fields
            ClearFormFields();

            // Remove any selection in the grid so inputs remain empty until user selects a row
            ClearGridSelection();
        }

        // add this new handler to the Form1 class

        private void ItemList_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Prevent the selection-change handler from reacting while we clear selection
            suppressSelectionEvents = true;
            try
            {
                // Ensure no current item in the BindingSource and no current cell in grid
                usersBinding.Position = -1;
                ItemList.ClearSelection();
                try { ItemList.CurrentCell = null; } catch { }

                // Move focus away from grid so it doesn't appear selected.
                // Prefer the search box if present.
                if (searchTextBox != null && searchTextBox.CanFocus)
                    searchTextBox.Focus();
                else
                    this.ActiveControl = searchTextBox != null ? searchTextBox : this;
            }
            finally
            {
                suppressSelectionEvents = false;
            }
        }
    }
}
