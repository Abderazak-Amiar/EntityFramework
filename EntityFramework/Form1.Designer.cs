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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            ItemList = new DataGridView();
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
            label7 = new Label();
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
            tabStatistics = new TabPage();
            lblStatsSummary = new Label();
            btnRefreshStats = new Button();
            dgvStats = new DataGridView();
            colMetric = new DataGridViewTextBoxColumn();
            colValue = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)ItemList).BeginInit();
            mainTabControl.SuspendLayout();
            tabClients.SuspendLayout();
            tabParameters.SuspendLayout();
            tabStatistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStats).BeginInit();
            SuspendLayout();
            // 
            // ItemList
            // 
            ItemList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ItemList.BackgroundColor = SystemColors.ControlLight;
            ItemList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ItemList.Location = new Point(30, 42);
            ItemList.MultiSelect = false;
            ItemList.Name = "ItemList";
            ItemList.Size = new Size(1100, 350);
            ItemList.TabIndex = 0;
            ItemList.CellContentClick += ItemList_CellContentClick;
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
            label3.Location = new Point(261, 436);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 11;
            label3.Text = "N° Tel";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(259, 465);
            label4.Name = "label4";
            label4.Size = new Size(53, 15);
            label4.TabIndex = 12;
            label4.Text = "Nbr Sacs";
            label4.Click += label4_Click;
            // 
            // textBox1
            // 
            textBox1.Enabled = false;
            textBox1.Location = new Point(331, 432);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(150, 23);
            textBox1.TabIndex = 13;
            textBox1.TextChanged += textBox1_TextChanged_1;
            // 
            // textBox2
            // 
            textBox2.Enabled = false;
            textBox2.Location = new Point(331, 461);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(150, 23);
            textBox2.TabIndex = 14;
            // 
            // textBox3
            // 
            textBox3.Enabled = false;
            textBox3.Location = new Point(579, 429);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(150, 23);
            textBox3.TabIndex = 15;
            // 
            // textBox4
            // 
            textBox4.Enabled = false;
            textBox4.Location = new Point(579, 458);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(150, 23);
            textBox4.TabIndex = 16;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(507, 433);
            label5.Name = "label5";
            label5.Size = new Size(66, 15);
            label5.TabIndex = 17;
            label5.Text = "Nbr Bidons";
            label5.Click += label5_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(507, 463);
            label6.Name = "label6";
            label6.Size = new Size(35, 15);
            label6.TabIndex = 18;
            label6.Text = "Litres";
            label6.Click += label6_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(258, 494);
            label7.Name = "label7";
            label7.Size = new Size(67, 15);
            label7.TabIndex = 19;
            label7.Text = "Net à Payer";
            // 
            // textBox5
            // 
            textBox5.Enabled = false;
            textBox5.Location = new Point(331, 490);
            textBox5.Name = "textBox5";
            textBox5.PlaceholderText = "Prix uinitaire";
            textBox5.Size = new Size(80, 23);
            textBox5.TabIndex = 20;
            // 
            // textBox6
            // 
            textBox6.Enabled = false;
            textBox6.Location = new Point(417, 490);
            textBox6.Name = "textBox6";
            textBox6.PlaceholderText = "Litres";
            textBox6.Size = new Size(64, 23);
            textBox6.TabIndex = 21;
            textBox6.TextChanged += textBox6_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(507, 493);
            label8.Name = "label8";
            label8.Size = new Size(32, 15);
            label8.TabIndex = 22;
            label8.Text = "Total";
            label8.Click += label8_Click;
            // 
            // textBox7
            // 
            textBox7.Enabled = false;
            textBox7.Location = new Point(579, 488);
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
            clearBtn.Location = new Point(100, 398);
            clearBtn.Name = "clearBtn";
            clearBtn.Size = new Size(75, 23);
            clearBtn.TabIndex = 28;
            clearBtn.Text = "Refresher";
            clearBtn.UseVisualStyleBackColor = true;
            clearBtn.Click += clearBtn_Click;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(tabClients);
            mainTabControl.Controls.Add(tabParameters);
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
            tabClients.Location = new Point(4, 24);
            tabClients.Name = "tabClients";
            tabClients.Padding = new Padding(3);
            tabClients.Size = new Size(1152, 709);
            tabClients.TabIndex = 0;
            tabClients.Text = "Clients";
            tabClients.UseVisualStyleBackColor = true;
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
            lblPortion.Size = new Size(46, 15);
            lblPortion.TabIndex = 6;
            lblPortion.Text = "Portion";
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
            // tabStatistics
            // 
            tabStatistics.Controls.Add(lblStatsSummary);
            tabStatistics.Controls.Add(btnRefreshStats);
            tabStatistics.Controls.Add(dgvStats);
            tabStatistics.Location = new Point(4, 24);
            tabStatistics.Name = "tabStatistics";
            tabStatistics.Padding = new Padding(3);
            tabStatistics.Size = new Size(1152, 709);
            tabStatistics.TabIndex = 2;
            tabStatistics.Text = "Statistiques";
            tabStatistics.UseVisualStyleBackColor = true;
            // 
            // lblStatsSummary
            // 
            lblStatsSummary.AutoSize = true;
            lblStatsSummary.Location = new Point(24, 24);
            lblStatsSummary.Name = "lblStatsSummary";
            lblStatsSummary.Size = new Size(166, 15);
            lblStatsSummary.TabIndex = 0;
            lblStatsSummary.Text = "Statistiques : (aucune donnée)";
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
            colMetric.Name = "colMetric";
            colMetric.ReadOnly = true;
            // 
            // colValue
            // 
            colValue.Name = "colValue";
            colValue.ReadOnly = true;
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
            tabStatistics.ResumeLayout(false);
            tabStatistics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStats).EndInit();
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
        private Label label7;
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

        // Statistics tab controls
        private Label lblStatsSummary;
        private Button btnRefreshStats;

        // NEW: statistics grid
        private DataGridView dgvStats;
        private DataGridViewTextBoxColumn colMetric;
        private DataGridViewTextBoxColumn colValue;
        private TextBox txtCompanyPhone;
        private Label label10;
    }
}
