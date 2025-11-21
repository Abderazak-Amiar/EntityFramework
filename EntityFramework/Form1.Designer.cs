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
            Poids = new Label();
            weightTextBox = new TextBox();
            label9 = new Label();
            searchTextBox = new TextBox();
            clearBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)ItemList).BeginInit();
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
            editCheckBox.CheckedChanged += checkBox1_CheckedChanged;
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
            nameTextBox.Location = new Point(100, 432);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(150, 23);
            nameTextBox.TabIndex = 4;
            nameTextBox.TextChanged += textBox1_TextChanged;
            // 
            // addressTextBox
            // 
            addressTextBox.Location = new Point(100, 461);
            addressTextBox.Name = "addressTextBox";
            addressTextBox.Size = new Size(150, 23);
            addressTextBox.TabIndex = 5;
            addressTextBox.TextChanged += textBox2_TextChanged;
            // 
            // createBtn
            // 
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
            textBox1.Location = new Point(331, 432);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(150, 23);
            textBox1.TabIndex = 13;
            textBox1.TextChanged += textBox1_TextChanged_1;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(331, 461);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(150, 23);
            textBox2.TabIndex = 14;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(579, 429);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(150, 23);
            textBox3.TabIndex = 15;
            // 
            // textBox4
            // 
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
            textBox5.Location = new Point(331, 490);
            textBox5.Name = "textBox5";
            textBox5.PlaceholderText = "Prix uinitaire";
            textBox5.Size = new Size(80, 23);
            textBox5.TabIndex = 20;
            // 
            // textBox6
            // 
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
            textBox7.Location = new Point(579, 488);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(150, 23);
            textBox7.TabIndex = 23;
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
            // weightTextBox
            // 
            weightTextBox.Location = new Point(100, 490);
            weightTextBox.Name = "weightTextBox";
            weightTextBox.Size = new Size(150, 23);
            weightTextBox.TabIndex = 25;
            weightTextBox.TextChanged += weightTextBox_TextChanged;
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
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 761);
            Controls.Add(clearBtn);
            Controls.Add(searchTextBox);
            Controls.Add(label9);
            Controls.Add(weightTextBox);
            Controls.Add(Poids);
            Controls.Add(textBox7);
            Controls.Add(label8);
            Controls.Add(textBox6);
            Controls.Add(textBox5);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(textBox4);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(printBtn);
            Controls.Add(deleteBtn);
            Controls.Add(updateBtn);
            Controls.Add(createBtn);
            Controls.Add(addressTextBox);
            Controls.Add(nameTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(editCheckBox);
            Controls.Add(ItemList);
            Name = "Form1";
            Text = "GESTION CLIENTS - HUILERIE BELABBAS ";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)ItemList).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
    }
}
