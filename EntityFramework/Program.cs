namespace EntityFramework
{
    using System;
    using System.Windows.Forms;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using QuestPDF.Infrastructure;

    internal static class Program
    {
        [STAThread]
        internal static void Main()
        {
            // Configure QuestPDF license if needed
            QuestPDF.Settings.License = LicenseType.Community;

            // Apply EF migrations at startup (creates db if needed)
            using (var context = new DataContext())
            {
                context.Database.Migrate();
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
// The ENC0118 diagnostic means that editing the 'Main' method during a debugging session (Edit and Continue) will not take effect until the application is restarted.
// This is a limitation of Edit and Continue for entry point methods like 'Main'.
// To resolve: Stop debugging, rebuild, and restart the application to apply changes to 'Main'.
// No code changes are required to fix this diagnostic; it is informational.
