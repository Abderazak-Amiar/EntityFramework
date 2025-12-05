using System;
using System.Drawing;
using System.Windows.Forms;

namespace EntityFramework
{
    internal class LicenseForm : Form
    {
        private readonly TextBox txtLicense;
        private readonly Button btnOk;
        private readonly Button btnCancel;
        public string? LicenseKey => txtLicense.Text;

        public LicenseForm(string? currentLicense)
        {
            Text = "Entrez la clé de licence";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 140);

            var lbl = new Label
            {
                Text = "Entrez la clé de licence:",
                AutoSize = false,
                Location = new Point(12, 12),
                Size = new Size(396, 32)
            };
            Controls.Add(lbl);

            txtLicense = new TextBox
            {
                Location = new Point(12, 48),
                Size = new Size(396, 24),
                Text = currentLicense ?? string.Empty
            };
            Controls.Add(txtLicense);

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(240, 88),
                Size = new Size(80, 28)
            };
            Controls.Add(btnOk);

            btnCancel = new Button
            {
                Text = "Annuler",
                DialogResult = DialogResult.Cancel,
                Location = new Point(328, 88),
                Size = new Size(80, 28)
            };
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}