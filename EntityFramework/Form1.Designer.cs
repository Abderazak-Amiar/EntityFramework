namespace EntityFramework
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            ItemList = new DataGridView();
            colNumber = new DataGridViewTextBoxColumn();
            editCheckBox = new CheckBox();
            label1 = new Label();
            label2 = new Label();
            nameTextBox = new TextBox();
            addressTextBox = new TextBox();
            createBtn = new Button();
            updateBtn = new Button();
            deleteBtn = new Button();
            printBtn = new Button();
            label3 = new Label();
            label4 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            textBox4 = new TextBox();
            label5 = new Label();
            label6 = new Label();
            textBox5 = new TextBox();
            textBox6 = new TextBox();
            label8 = new Label();
            textBox7 = new TextBox();
            label9 = new Label();
            weightTextBox = new TextBox();
            Poids = new Label();
            searchTextBox = new TextBox();
            clearBtn = new Button();
            mainTabControl = new TabControl();
            tabClients = new TabPage();
            label11 = new Label();
            label7 = new Label();
            rdoPortion = new RadioButton();
            rdoPaiement = new RadioButton();
            tabParameters = new TabPage();
            txtCompanyPhone = new TextBox();
            label10 = new Label();
            lblCompanyName = new Label();
            txtCompanyName = new TextBox();
            lblCompanyAddressPhone = new Label();
            txtCompanyAddress = new TextBox();
            lblPricePerLiter = new Label();
            txtPricePerLiter = new TextBox();
            lblPortion = new Label();
            txtPortion = new TextBox();
            btnSaveParameters = new Button();
            ParametersCheckBox = new CheckBox();
            tabVente = new TabPage();
            btnEnregistrerVente = new Button();
            txtVentePrix = new TextBox();
            lblVentePrix = new Label();
            txtVenteNbrLitres = new TextBox();
            lblVenteNbrLitres = new Label();
            lblVenteMontant = new Label();
            lblVenteMontantValue = new Label();
            dgvVentesToday = new DataGridView();
            colVenteId = new DataGridViewTextBoxColumn();
            colVenteTime = new DataGridViewTextBoxColumn();
            colVenteLitres = new DataGridViewTextBoxColumn();
            colVentePrix = new DataGridViewTextBoxColumn();
            colVenteMontant = new DataGridViewTextBoxColumn();
            colVenteDelete = new DataGridViewButtonColumn();
            chkPrintReceipt = new CheckBox();
            lblVenteToast = new Label();
            tabStatistics = new TabPage();
            yearComboBox = new ComboBox();
            btnRefreshStats = new Button();
            dgvStats = new DataGridView();
            colMetric = new DataGridViewTextBoxColumn();
            colValue = new DataGridViewTextBoxColumn();
            dgvVenteStats = new DataGridView();
            colVenteMetric = new DataGridViewTextBoxColumn();
            colVenteValue = new DataGridViewTextBoxColumn();
            toastTimer = new System.Windows.Forms.Timer(components);
            modeComboBox = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)ItemList).BeginInit();
            mainTabControl.SuspendLayout();
            tabClients.SuspendLayout();
            tabParameters.SuspendLayout();
            tabVente.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVentesToday).BeginInit();
            tabStatistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStats).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvVenteStats).BeginInit();
            SuspendLayout();
            // 
            // ItemList
            // 
            ItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ItemList.BackgroundColor = SystemColors.ControlLight;
            ItemList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ItemList.Columns.AddRange(new DataGridViewColumn[] { colNumber });
            ItemList.Location = new Point(30, 42);
            ItemList.MultiSelect = false;
            ItemList.Name = "ItemList";
            ItemList.Size = new Size(1100, 350);
            ItemList.TabIndex = 0;
            ItemList.CellContentClick += ItemList_CellContentClick;
            // 
            // colNumber
            // 
            colNumber.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colNumber.HeaderText = "N°";
            colNumber.Name = "colNumber";
            colNumber.ReadOnly = true;
            colNumber.Width = 46;
            // 
            // editCheckBox
            // 
            editCheckBox.AutoSize = true;
            editCheckBox.Location = new Point(30, 399);
            editCheckBox.Name = "editCheckBox";
            editCheckBox.Size = new Size(56, 19);
            editCheckBox.TabIndex = 1;
            editCheckBox.Text = "Editer";
            editCheckBox.UseVisualStyleBackColor = true;
            editCheckBox.CheckedChanged += editCheckBox_CheckedChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 436);
            label1.Name = "label1";
            label1.Size = new Size(34, 15);
            label1.TabIndex = 2;
            label1.Text = "Nom";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 465);
            label2.Name = "label2";
            label2.Size = new Size(49, 15);
            label2.TabIndex = 3;
            label2.Text = "Address";
            label2.Click += label2_Click;
            // 
            // nameTextBox
            // 
            nameTextBox.Enabled = false;
            nameTextBox.Location = new Point(100, 432);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(150, 23);
            nameTextBox.TabIndex = 4;
            nameTextBox.TextChanged += textBox1_TextChanged;
            // 
            // addressTextBox
            // 
            addressTextBox.Enabled = false;
            addressTextBox.Location = new Point(100, 461);
            addressTextBox.Name = "addressTextBox";
            addressTextBox.Size = new Size(150, 23);
            addressTextBox.TabIndex = 5;
            addressTextBox.TextChanged += textBox2_TextChanged;
            // 
            // createBtn
            // 
            createBtn.Enabled = false;
            createBtn.Location = new Point(104, 561);
            createBtn.Name = "createBtn";
            createBtn.Size = new Size(75, 23);
            createBtn.TabIndex = 6;
            createBtn.Text = "Créer";
            createBtn.UseVisualStyleBackColor = true;
            createBtn.Click += createBtn_Click;
            // 
            // updateBtn
            // 
            updateBtn.Enabled = false;
            updateBtn.Location = new Point(202, 561);
            updateBtn.Name = "updateBtn";
            updateBtn.Size = new Size(75, 23);
            updateBtn.TabIndex = 8;
            updateBtn.Text = "Modifier";
            updateBtn.UseVisualStyleBackColor = true;
            updateBtn.Click += updateBtn_Click;
            // 
            // deleteBtn
            // 
            deleteBtn.Enabled = false;
            deleteBtn.Location = new Point(300, 561);
            deleteBtn.Name = "deleteBtn";
            deleteBtn.Size = new Size(75, 23);
            deleteBtn.TabIndex = 9;
            deleteBtn.Text = "Supprimer";
            deleteBtn.UseVisualStyleBackColor = true;
            deleteBtn.Click += deleteBtn_Click;
            // 
            // printBtn
            // 
            printBtn.Location = new Point(398, 561);
            printBtn.Name = "printBtn";
            printBtn.Size = new Size(75, 23);
            printBtn.TabIndex = 10;
            printBtn.Text = "Imprimer";
            printBtn.UseVisualStyleBackColor = true;
            printBtn.Click += printBtn_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(276, 436);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 11;
            label3.Text = "N° Tel";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(274, 465);
            label4.Name = "label4";
            label4.Size = new Size(30, 15);
            label4.TabIndex = 12;
            label4.Text = "Sacs";
            label4.Click += label4_Click;
            // 
            // textBox1
            // 
            textBox1.Enabled = false;
            textBox1.Location = new Point(375, 432);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(150, 23);
            textBox1.TabIndex = 13;
            textBox1.TextChanged += textBox1_TextChanged_1;
            // 
            // textBox2
            // 
            textBox2.Enabled = false;
            textBox2.Location = new Point(375, 461);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(150, 23);
            textBox2.TabIndex = 14;
            // 
            // textBox3
            // 
            textBox3.Enabled = false;
            textBox3.Location = new Point(624, 429);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(150, 23);
            textBox3.TabIndex = 15;
            // 
            // textBox4
            // 
            textBox4.Enabled = false;
            textBox4.Location = new Point(624, 460);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(150, 23);
            textBox4.TabIndex = 16;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(552, 433);
            label5.Name = "label5";
            label5.Size = new Size(43, 15);
            label5.TabIndex = 17;
            label5.Text = "Bidons";
            label5.Click += label5_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(552, 463);
            label6.Name = "label6";
            label6.Size = new Size(67, 15);
            label6.TabIndex = 18;
            label6.Text = "Quantité(L)";
            label6.Click += label6_Click;
            // 
            // textBox5
            // 
            textBox5.Enabled = false;
            textBox5.Location = new Point(462, 491);
            textBox5.Name = "textBox5";
            textBox5.PlaceholderText = "Prix uinitaire";
            textBox5.Size = new Size(63, 23);
            textBox5.TabIndex = 20;
            // 
            // textBox6
            // 
            textBox6.Enabled = false;
            textBox6.Location = new Point(375, 491);
            textBox6.Name = "textBox6";
            textBox6.PlaceholderText = "Litres";
            textBox6.Size = new Size(47, 23);
            textBox6.TabIndex = 21;
            textBox6.TextChanged += textBox6_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(552, 494);
            label8.Name = "label8";
            label8.Size = new Size(32, 15);
            label8.TabIndex = 22;
            label8.Text = "Total";
            label8.Click += label8_Click;
            // 
            // textBox7
            // 
            textBox7.Enabled = false;
            textBox7.Location = new Point(624, 490);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(150, 23);
            textBox7.TabIndex = 23;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(26, 17);
            label9.Name = "label9";
            label9.Size = new Size(62, 15);
            label9.TabIndex = 26;
            label9.Text = "Recherche";
            label9.Click += label9_Click;
            // 
            // weightTextBox
            // 
            weightTextBox.Enabled = false;
            weightTextBox.Location = new Point(100, 490);
            weightTextBox.Name = "weightTextBox";
            weightTextBox.Size = new Size(150, 23);
            weightTextBox.TabIndex = 25;
            weightTextBox.TextChanged += weightTextBox_TextChanged;
            // 
            // Poids
            // 
            Poids.AutoSize = true;
            Poids.Location = new Point(30, 496);
            Poids.Name = "Poids";
            Poids.Size = new Size(36, 15);
            Poids.TabIndex = 24;
            Poids.Text = "Poids";
            Poids.Click += Poids_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Location = new Point(100, 13);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new Size(150, 23);
            searchTextBox.TabIndex = 27;
            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            // 
            // clearBtn
            // 
            clearBtn.Location = new Point(497, 561);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(75, 23);
            clearBtn.TabIndex = 28;
            clearBtn.Text = "Rafraîchir";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += clearBtn_Click;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(tabClients);
            mainTabControl.Controls.Add(tabParameters);
            mainTabControl.Controls.Add(tabVente);
            mainTabControl.Controls.Add(tabStatistics);
            mainTabControl.Location = new Point(12, 12);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(1160, 737);
            mainTabControl.TabIndex = 0;
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            // 
            // tabClients
            // 
            tabClients.Controls.Add(label11);
            tabClients.Controls.Add(clearBtn);
            tabClients.Controls.Add(searchTextBox);
            tabClients.Controls.Add(label9);
            tabClients.Controls.Add(weightTextBox);
            tabClients.Controls.Add(Poids);
            tabClients.Controls.Add(textBox7);
            tabClients.Controls.Add(label8);
            tabClients.Controls.Add(textBox6);
            tabClients.Controls.Add(textBox5);
            tabClients.Controls.Add(label7);
            tabClients.Controls.Add(label6);
            tabClients.Controls.Add(label5);
            tabClients.Controls.Add(textBox4);
            tabClients.Controls.Add(textBox3);
            tabClients.Controls.Add(textBox2);
            tabClients.Controls.Add(textBox1);
            tabClients.Controls.Add(label4);
            tabClients.Controls.Add(label3);
            tabClients.Controls.Add(printBtn);
            tabClients.Controls.Add(deleteBtn);
            tabClients.Controls.Add(updateBtn);
            tabClients.Controls.Add(createBtn);
            tabClients.Controls.Add(addressTextBox);
            tabClients.Controls.Add(nameTextBox);
            tabClients.Controls.Add(label2);
            tabClients.Controls.Add(label1);
            tabClients.Controls.Add(editCheckBox);
            tabClients.Controls.Add(ItemList);
            tabClients.Controls.Add(rdoPortion);
            tabClients.Controls.Add(rdoPaiement);
            tabClients.Location = new Point(4, 24);
            tabClients.Name = "tabClients";
            tabClients.Padding = new Padding(3);
            tabClients.Size = new Size(1152, 709);
            tabClients.TabIndex = 0;
            tabClients.Text = "Clients";
            tabClients.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(424, 494);
            label11.Name = "label11";
            label11.Size = new Size(38, 15);
            label11.TabIndex = 32;
            label11.Text = "Prix/L";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(273, 494);
            label7.Name = "label7";
            label7.Size = new Size(76, 15);
            label7.TabIndex = 19;
            label7.Text = "Litres à payer";
            label7.Click += label7_Click;
            // 
            // rdoPortion
            // 
            rdoPortion.AutoSize = true;
            rdoPortion.Location = new Point(99, 397);
            rdoPortion.Name = "rdoPortion";
            rdoPortion.Size = new Size(64, 19);
            rdoPortion.TabIndex = 30;
            rdoPortion.TabStop = true;
            rdoPortion.Text = "Portion";
            rdoPortion.UseVisualStyleBackColor = true;
            rdoPortion.CheckedChanged += rdoMode_CheckedChanged;
            // 
            // rdoPaiement
            // 
            rdoPaiement.AutoSize = true;
            rdoPaiement.Location = new Point(180, 397);
            rdoPaiement.Name = "rdoPaiement";
            rdoPaiement.Size = new Size(75, 19);
            rdoPaiement.TabIndex = 31;
            rdoPaiement.TabStop = true;
            rdoPaiement.Text = "Paiement";
            rdoPaiement.UseVisualStyleBackColor = true;
            rdoPaiement.CheckedChanged += rdoMode_CheckedChanged;
            // 
            // tabParameters
            // 
            tabParameters.Controls.Add(txtCompanyPhone);
            tabParameters.Controls.Add(label10);
            tabParameters.Controls.Add(lblCompanyName);
            tabParameters.Controls.Add(txtCompanyName);
            tabParameters.Controls.Add(lblCompanyAddressPhone);
            tabParameters.Controls.Add(txtCompanyAddress);
            tabParameters.Controls.Add(lblPricePerLiter);
            tabParameters.Controls.Add(txtPricePerLiter);
            tabParameters.Controls.Add(lblPortion);
            tabParameters.Controls.Add(txtPortion);
            tabParameters.Controls.Add(btnSaveParameters);
            tabParameters.Controls.Add(ParametersCheckBox);
            tabParameters.Location = new Point(4, 24);
            tabParameters.Name = "tabParameters";
            tabParameters.Padding = new Padding(3);
            tabParameters.Size = new Size(1152, 709);
            tabParameters.TabIndex = 1;
            tabParameters.Text = "Paramètres";
            tabParameters.UseVisualStyleBackColor = true;
            // 
            // txtCompanyPhone
            // 
            txtCompanyPhone.Location = new Point(160, 110);
            txtCompanyPhone.Name = "txtCompanyPhone";
            txtCompanyPhone.Size = new Size(98, 23);
            txtCompanyPhone.TabIndex = 11;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(24, 110);
            label10.Name = "label10";
            label10.Size = new Size(61, 15);
            label10.TabIndex = 10;
            label10.Text = "Téléphone";
            // 
            // lblCompanyName
            // 
            lblCompanyName.AutoSize = true;
            lblCompanyName.Location = new Point(24, 45);
            lblCompanyName.Name = "lblCompanyName";
            lblCompanyName.Size = new Size(89, 15);
            lblCompanyName.TabIndex = 0;
            lblCompanyName.Text = "Nom entreprise";
            // 
            // txtCompanyName
            // 
            txtCompanyName.Enabled = false;
            txtCompanyName.Location = new Point(160, 41);
            txtCompanyName.Name = "txtCompanyName";
            txtCompanyName.Size = new Size(360, 23);
            txtCompanyName.TabIndex = 1;
            // 
            // lblCompanyAddressPhone
            // 
            lblCompanyAddressPhone.AutoSize = true;
            lblCompanyAddressPhone.Location = new Point(24, 80);
            lblCompanyAddressPhone.Name = "lblCompanyAddressPhone";
            lblCompanyAddressPhone.Size = new Size(48, 15);
            lblCompanyAddressPhone.TabIndex = 2;
            lblCompanyAddressPhone.Text = "Adresse";
            lblCompanyAddressPhone.Click += lblCompanyAddressPhone_Click;
            // 
            // txtCompanyAddress
            // 
            txtCompanyAddress.Enabled = false;
            txtCompanyAddress.Location = new Point(160, 77);
            txtCompanyAddress.Name = "txtCompanyAddress";
            txtCompanyAddress.Size = new Size(360, 23);
            txtCompanyAddress.TabIndex = 3;
            txtCompanyAddress.TextChanged += txtCompanyAddressPhone_TextChanged;
            // 
            // lblPricePerLiter
            // 
            lblPricePerLiter.AutoSize = true;
            lblPricePerLiter.Location = new Point(24, 147);
            lblPricePerLiter.Name = "lblPricePerLiter";
            lblPricePerLiter.Size = new Size(38, 15);
            lblPricePerLiter.TabIndex = 4;
            lblPricePerLiter.Text = "Prix/L";
            // 
            // txtPricePerLiter
            // 
            txtPricePerLiter.Enabled = false;
            txtPricePerLiter.Location = new Point(160, 143);
            txtPricePerLiter.Name = "txtPricePerLiter";
            txtPricePerLiter.Size = new Size(100, 23);
            txtPricePerLiter.TabIndex = 5;
            // 
            // lblPortion
            // 
            lblPortion.AutoSize = true;
            lblPortion.Location = new Point(24, 183);
            lblPortion.Name = "lblPortion";
            lblPortion.Size = new Size(64, 15);
            lblPortion.TabIndex = 6;
            lblPortion.Text = "Portion(%)";
            lblPortion.Click += lblPortion_Click;
            // 
            // txtPortion
            // 
            txtPortion.Enabled = false;
            txtPortion.Location = new Point(160, 179);
            txtPortion.Name = "txtPortion";
            txtPortion.Size = new Size(100, 23);
            txtPortion.TabIndex = 7;
            // 
            // btnSaveParameters
            // 
            btnSaveParameters.Enabled = false;
            btnSaveParameters.Location = new Point(24, 219);
            btnSaveParameters.Name = "btnSaveParameters";
            btnSaveParameters.Size = new Size(120, 27);
            btnSaveParameters.TabIndex = 8;
            btnSaveParameters.Text = "Enregistrer";
            btnSaveParameters.Click += BtnSaveParameters_Click;
            // 
            // ParametersCheckBox
            // 
            ParametersCheckBox.AutoSize = true;
            ParametersCheckBox.Location = new Point(28, 10);
            ParametersCheckBox.Name = "ParametersCheckBox";
            ParametersCheckBox.Size = new Size(56, 19);
            ParametersCheckBox.TabIndex = 9;
            ParametersCheckBox.Text = "Editer";
            ParametersCheckBox.UseVisualStyleBackColor = true;
            ParametersCheckBox.CheckedChanged += ParametersCheckBox_CheckedChanged;
            // 
            // tabVente
            // 
            tabVente.Controls.Add(btnEnregistrerVente);
            tabVente.Controls.Add(txtVentePrix);
            tabVente.Controls.Add(lblVentePrix);
            tabVente.Controls.Add(txtVenteNbrLitres);
            tabVente.Controls.Add(lblVenteNbrLitres);
            tabVente.Controls.Add(lblVenteMontant);
            tabVente.Controls.Add(lblVenteMontantValue);
            tabVente.Controls.Add(dgvVentesToday);
            tabVente.Controls.Add(chkPrintReceipt);
            tabVente.Controls.Add(lblVenteToast);
            tabVente.Location = new Point(4, 24);
            tabVente.Name = "tabVente";
            tabVente.Padding = new Padding(3);
            tabVente.Size = new Size(1152, 709);
            tabVente.TabIndex = 3;
            tabVente.Text = "Vente";
            tabVente.UseVisualStyleBackColor = true;
            // 
            // btnEnregistrerVente
            // 
            btnEnregistrerVente.Location = new Point(24, 100);
            btnEnregistrerVente.Name = "btnEnregistrerVente";
            btnEnregistrerVente.Size = new Size(100, 27);
            btnEnregistrerVente.TabIndex = 4;
            btnEnregistrerVente.Text = "Enregistrer";
            btnEnregistrerVente.UseVisualStyleBackColor = true;
            btnEnregistrerVente.Click += btnEnregistrerVente_Click;
            // 
            // txtVentePrix
            // 
            txtVentePrix.Location = new Point(130, 60);
            txtVentePrix.Name = "txtVentePrix";
            txtVentePrix.Size = new Size(120, 23);
            txtVentePrix.TabIndex = 3;
            txtVentePrix.TextChanged += VenteFields_TextChanged;
            // 
            // lblVentePrix
            // 
            lblVentePrix.AutoSize = true;
            lblVentePrix.Location = new Point(24, 64);
            lblVentePrix.Name = "lblVentePrix";
            lblVentePrix.Size = new Size(44, 15);
            lblVentePrix.TabIndex = 2;
            lblVentePrix.Text = "Prix/L :";
            // 
            // txtVenteNbrLitres
            // 
            txtVenteNbrLitres.Location = new Point(130, 24);
            txtVenteNbrLitres.Name = "txtVenteNbrLitres";
            txtVenteNbrLitres.Size = new Size(120, 23);
            txtVenteNbrLitres.TabIndex = 1;
            txtVenteNbrLitres.TextChanged += VenteFields_TextChanged;
            // 
            // lblVenteNbrLitres
            // 
            lblVenteNbrLitres.AutoSize = true;
            lblVenteNbrLitres.Location = new Point(24, 28);
            lblVenteNbrLitres.Name = "lblVenteNbrLitres";
            lblVenteNbrLitres.Size = new Size(81, 15);
            lblVenteNbrLitres.TabIndex = 0;
            lblVenteNbrLitres.Text = "Nbr Litres (L) :";
            // 
            // lblVenteMontant
            // 
            lblVenteMontant.AutoSize = true;
            lblVenteMontant.Location = new Point(24, 140);
            lblVenteMontant.Name = "lblVenteMontant";
            lblVenteMontant.Size = new Size(53, 15);
            lblVenteMontant.TabIndex = 6;
            lblVenteMontant.Text = "Montant";
            // 
            // lblVenteMontantValue
            // 
            lblVenteMontantValue.AutoSize = true;
            lblVenteMontantValue.Location = new Point(130, 140);
            lblVenteMontantValue.Name = "lblVenteMontantValue";
            lblVenteMontantValue.Size = new Size(0, 15);
            lblVenteMontantValue.TabIndex = 7;
            // 
            // dgvVentesToday
            // 
            dgvVentesToday.AllowUserToAddRows = false;
            dgvVentesToday.AllowUserToDeleteRows = false;
            dgvVentesToday.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVentesToday.Columns.AddRange(new DataGridViewColumn[] { colVenteId, colVenteTime, colVenteLitres, colVentePrix, colVenteMontant, colVenteDelete });
            dgvVentesToday.Location = new Point(24, 180);
            dgvVentesToday.Name = "dgvVentesToday";
            dgvVentesToday.ReadOnly = true;
            dgvVentesToday.RowHeadersVisible = false;
            dgvVentesToday.Size = new Size(700, 260);
            dgvVentesToday.TabIndex = 8;
            dgvVentesToday.CellContentClick += DgvVentesToday_CellContentClick;
            // 
            // colVenteId
            // 
            colVenteId.Name = "colVenteId";
            colVenteId.ReadOnly = true;
            // 
            // colVenteTime
            // 
            colVenteTime.Name = "colVenteTime";
            colVenteTime.ReadOnly = true;
            // 
            // colVenteLitres
            // 
            colVenteLitres.Name = "colVenteLitres";
            colVenteLitres.ReadOnly = true;
            // 
            // colVentePrix
            // 
            colVentePrix.Name = "colVentePrix";
            colVentePrix.ReadOnly = true;
            // 
            // colVenteMontant
            // 
            colVenteMontant.Name = "colVenteMontant";
            colVenteMontant.ReadOnly = true;
            // 
            // colVenteDelete
            // 
            colVenteDelete.Name = "colVenteDelete";
            colVenteDelete.ReadOnly = true;
            // 
            // chkPrintReceipt
            // 
            chkPrintReceipt.AutoSize = true;
            chkPrintReceipt.Location = new Point(140, 104);
            chkPrintReceipt.Name = "chkPrintReceipt";
            chkPrintReceipt.Size = new Size(107, 19);
            chkPrintReceipt.TabIndex = 5;
            chkPrintReceipt.Text = "Imprimer ticket";
            chkPrintReceipt.UseVisualStyleBackColor = true;
            // 
            // lblVenteToast
            // 
            lblVenteToast.BackColor = Color.FromArgb(255, 250, 205);
            lblVenteToast.BorderStyle = BorderStyle.FixedSingle;
            lblVenteToast.ForeColor = Color.Black;
            lblVenteToast.Location = new Point(24, 460);
            lblVenteToast.Name = "lblVenteToast";
            lblVenteToast.Padding = new Padding(6);
            lblVenteToast.Size = new Size(360, 28);
            lblVenteToast.TabIndex = 20;
            lblVenteToast.Visible = false;
            // 
            // tabStatistics
            // 
            tabStatistics.Controls.Add(yearComboBox);
            tabStatistics.Controls.Add(btnRefreshStats);
            tabStatistics.Controls.Add(dgvStats);
            tabStatistics.Controls.Add(dgvVenteStats);
            tabStatistics.Location = new Point(4, 24);
            tabStatistics.Name = "tabStatistics";
            tabStatistics.Padding = new Padding(3);
            tabStatistics.Size = new Size(1152, 709);
            tabStatistics.TabIndex = 2;
            tabStatistics.Text = "Statistiques";
            tabStatistics.UseVisualStyleBackColor = true;
            // 
            // yearComboBox
            // 
            yearComboBox.FormattingEnabled = true;
            yearComboBox.Location = new Point(24, 14);
            yearComboBox.Name = "yearComboBox";
            yearComboBox.Size = new Size(121, 23);
            yearComboBox.TabIndex = 3;
            // 
            // btnRefreshStats
            // 
            btnRefreshStats.Location = new Point(24, 56);
            btnRefreshStats.Name = "btnRefreshStats";
            btnRefreshStats.Size = new Size(100, 27);
            btnRefreshStats.TabIndex = 1;
            btnRefreshStats.Text = "Rafraîchir";
            btnRefreshStats.Click += BtnRefreshStats_Click;
            // 
            // dgvStats
            // 
            dgvStats.AllowUserToAddRows = false;
            dgvStats.AllowUserToDeleteRows = false;
            dgvStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvStats.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvStats.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStats.Columns.AddRange(new DataGridViewColumn[] { colMetric, colValue });
            dgvStats.EnableHeadersVisualStyles = false;
            dgvStats.Location = new Point(24, 96);
            dgvStats.Name = "dgvStats";
            dgvStats.ReadOnly = true;
            dgvStats.RowHeadersVisible = false;
            dgvStats.Size = new Size(700, 260);
            dgvStats.TabIndex = 2;
            // 
            // colMetric
            // 
            colMetric.HeaderText = "Métrique";
            colMetric.Name = "colMetric";
            colMetric.ReadOnly = true;
            // 
            // colValue
            // 
            colValue.HeaderText = "Valeur";
            colValue.Name = "colValue";
            colValue.ReadOnly = true;
            // 
            // dgvVenteStats
            // 
            dgvVenteStats.AllowUserToAddRows = false;
            dgvVenteStats.AllowUserToDeleteRows = false;
            dgvVenteStats.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvVenteStats.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvVenteStats.Columns.AddRange(new DataGridViewColumn[] { colVenteMetric, colVenteValue });
            dgvVenteStats.EnableHeadersVisualStyles = false;
            dgvVenteStats.Location = new Point(750, 96);
            dgvVenteStats.Name = "dgvVenteStats";
            dgvVenteStats.ReadOnly = true;
            dgvVenteStats.RowHeadersVisible = false;
            dgvVenteStats.Size = new Size(360, 260);
            dgvVenteStats.TabIndex = 4;
            // 
            // colVenteMetric
            // 
            colVenteMetric.HeaderText = "Métrique Vente";
            colVenteMetric.Name = "colVenteMetric";
            colVenteMetric.ReadOnly = true;
            // 
            // colVenteValue
            // 
            colVenteValue.HeaderText = "Valeur";
            colVenteValue.Name = "colVenteValue";
            colVenteValue.ReadOnly = true;
            // 
            // toastTimer
            // 
            toastTimer.Interval = 2000;
            toastTimer.Tick += ToastTimer_Tick;
            // 
            // modeComboBox
            // 
            modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            modeComboBox.FormattingEnabled = true;
            modeComboBox.Items.AddRange(new object[] { "Portion", "Paiement" });
            modeComboBox.Location = new Point(366, 490);
            modeComboBox.Name = "modeComboBox";
            modeComboBox.Size = new Size(120, 23);
            modeComboBox.TabIndex = 30;
            modeComboBox.SelectedIndexChanged += ModeComboBox_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(mainTabControl);
            Name = "Form1";
            Text = "GESTION CLIENTS - HUILERIE BELABBAS ";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)ItemList).EndInit();
            mainTabControl.ResumeLayout(false);
            tabClients.ResumeLayout(false);
            tabClients.PerformLayout();
            tabParameters.ResumeLayout(false);
            tabParameters.PerformLayout();
            tabVente.ResumeLayout(false);
            tabVente.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvVentesToday).EndInit();
            tabStatistics.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStats).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvVenteStats).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView ItemList;
        private CheckBox editCheckBox;
        private Label label1;
        private Label label2;
        private TextBox nameTextBox;
        private TextBox addressTextBox;
        private Button createBtn;
        private Button updateBtn;
        private Button deleteBtn;
        private Button printBtn;
        private Label label3;
        private Label label4;
        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private TextBox textBox4;
        private Label label5;
        private Label label6;
        private TextBox textBox5;
        private TextBox textBox6;
        private Label label8;
        private TextBox textBox7;
        private Label Poids;
        private TextBox weightTextBox;
        private Label label9;
        private TextBox searchTextBox;
        private Button clearBtn;

        // New fields (tabs)
        private TabControl mainTabControl;
        private TabPage tabClients;
        private TabPage tabParameters;
        private TabPage tabVente;
        private TabPage tabStatistics;

        // Parameters tab controls (requested)
        private Label lblCompanyName;
        private TextBox txtCompanyName;
        private Label lblCompanyAddressPhone;
        private TextBox txtCompanyAddress;
        private Label lblPricePerLiter;
        private TextBox txtPricePerLiter;
        private Label lblPortion;
        private TextBox txtPortion;
        private Button btnSaveParameters;

        // <-- New checkbox added for enabling/disabling parameter inputs -->
        private CheckBox ParametersCheckBox;

        // Vente tab controls (replaced ListBox with DataGridView)
        private Label lblVenteNbrLitres;
        private TextBox txtVenteNbrLitres;
        private Label lblVentePrix;
        private TextBox txtVentePrix;
        private Button btnEnregistrerVente;

        // Montant display
        private Label lblVenteMontant;
        private Label lblVenteMontantValue;

        // Replaced: ListBox -> DataGridView for today's ventes
        private DataGridView dgvVentesToday;

        // New hidden Id column + delete button column will be added in InitializeComponent

        // Print checkbox
        private CheckBox chkPrintReceipt;

        // Toast UI
        private Label lblVenteToast;
        private System.Windows.Forms.Timer toastTimer;

        // Statistics tab controls
        private Button btnRefreshStats;

        // NEW: statistics grid
        private DataGridView dgvStats;
        private DataGridViewTextBoxColumn colMetric;
        private DataGridViewTextBoxColumn colValue;
        private TextBox txtCompanyPhone;
        private Label label10;
        private ComboBox yearComboBox;

        // NEW: vente-specific stats grid
        private DataGridView dgvVenteStats;
        private DataGridViewTextBoxColumn colVenteMetric;
        private DataGridViewTextBoxColumn colVenteValue;

        // NEW: column for ItemList row numbering
        private DataGridViewTextBoxColumn colNumber;
        private DataGridViewTextBoxColumn colVenteId;
        private DataGridViewTextBoxColumn colVenteTime;
        private DataGridViewTextBoxColumn colVenteLitres;
        private DataGridViewTextBoxColumn colVentePrix;
        private DataGridViewTextBoxColumn colVenteMontant;
        private DataGridViewButtonColumn colVenteDelete;
        private Label label7;
        private ComboBox modeComboBox;
        private RadioButton rdoPortion;
        private RadioButton rdoPaiement;
        private Label label11;
    }
}
