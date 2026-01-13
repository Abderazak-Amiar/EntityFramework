using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using QuestPDF.Infrastructure;
using Serilog;

namespace EntityFramework
{
    internal static class Program
    {
        // RegisterApplicationRestart: allow Windows to restart the app after updates/crashes (best-effort)
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegisterApplicationRestart(string pwzCommandline, int dwFlags);

        [STAThread]
        internal static void Main()
        {
            // Best-effort: register for automatic restart after crash/OS updates.
            // Call early so Windows can record restart settings.
            try
            {
                RegisterApplicationRestart(null, 0);
            }
            catch
            {
                // ignore failures on platforms where API is unavailable
            }

            // Configure QuestPDF license if needed
            QuestPDF.Settings.License = LicenseType.Community;

            // Initialize WinForms application settings BEFORE creating any windows or showing dialogs.
            ApplicationConfiguration.Initialize();

            // Ensure native folder for runtime native dependencies is on PATH
            var procArch = RuntimeInformation.ProcessArchitecture;
            var nativeFolder = procArch == Architecture.X64 || procArch == Architecture.Arm64
                ? "runtimes\\win-x64\\native"
                : "runtimes\\win-x86\\native";

            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, nativeFolder);
            var current = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            Environment.SetEnvironmentVariable("PATH", path + Path.PathSeparator + current);

            // Apply EF migrations and perform license check
            using (var context = new DataContext())
            {
                context.Database.Migrate();

                // Defensive: ensure LicenseKey column exists in Parameters table
                try
                {
                    var connection = (SqliteConnection)context.Database.GetDbConnection();
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "PRAGMA table_info('Parameters')";
                        using var reader = cmd.ExecuteReader();
                        var hasLicenseKey = false;
                        while (reader.Read())
                        {
                            var name = reader.IsDBNull(1) ? null : reader.GetString(1);
                            if (string.Equals(name, "LicenseKey", StringComparison.OrdinalIgnoreCase))
                            {
                                hasLicenseKey = true;
                                break;
                            }
                        }

                        if (!hasLicenseKey)
                        {
                            using var addCmd = connection.CreateCommand();
                            addCmd.CommandText = "ALTER TABLE Parameters ADD COLUMN LicenseKey TEXT";
                            addCmd.ExecuteNonQuery();
                        }
                    }
                }
                catch
                {
                    // ignore -- we'll surface meaningful errors below when required
                }

                // Ensure a Parameters row exists (Id =1). If missing, create and persist it.
                var parameters = context.Parameters!.SingleOrDefault(p => p.Id == 1);
                if (parameters == null)
                {
                    parameters = new Parameters { Id = 1 };
                    context.Parameters!.Add(parameters);
                    context.SaveChanges();
                }

                // Always compare DB license key with machine license
                var machineSerial = GetMotherboardSerial()?.Trim();
                //Log.Information($"==>Machine Serial: {machineSerial}");

                if (string.IsNullOrEmpty(machineSerial))
                {
                    var serialDisplay = machineSerial == null ? "(null)" : $"\"{machineSerial}\" (length={machineSerial.Length})";
                    MessageBox.Show($"Impossible de lire le code matériel de la machine. L'application ne peut pas vérifier la licence.\nValeur retournée par GetMotherboardSerial(): {serialDisplay}", "Erreur licence", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // If no license in DB -> prompt user to enter one and validate against machine serial
                if (string.IsNullOrWhiteSpace(parameters.LicenseKey))
                {
                    bool validated = false;
                    while (!validated)
                    {
                        using var form = new LicenseForm(parameters.LicenseKey);
                        var res = form.ShowDialog();
                        if (res != DialogResult.OK)
                        {
                            // user cancelled -> exit application
                            return;
                        }

                        var entered = form.LicenseKey?.Trim();
                        //Log.Information($"==>Entered License: {entered}");
                        if (!string.IsNullOrEmpty(entered) &&
                            string.Equals(entered, machineSerial, StringComparison.OrdinalIgnoreCase))
                        {
                            parameters.LicenseKey = entered;
                            // parameters is tracked (added or queried), so just save
                            context.SaveChanges();
                            validated = true;
                            MessageBox.Show("Licence validée et enregistrée.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Le code licence n'est pas valide. Veuillez réessayer.", "Licence invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                else
                {
                    // License exists: compare with current machine serial
                    if (!string.Equals(parameters.LicenseKey?.Trim(), machineSerial, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("La licence enregistrée ne correspond pas à cette machine. Contactez le support.", "Licence invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // Continue application start
            using var ctx = new DataContext();
            var companyName = ctx.Parameters?.FirstOrDefault(p => p.Id == 1)?.CompanyName ?? string.Empty;

            Application.Run(new Form1());
        }

        // Read motherboard serial using WMI (Win32_BaseBoard only). Windows-only.
        public static string GetMotherboardSerial()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-WmiObject Win32_BaseBoard | Select -ExpandProperty SerialNumber",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            return result;
        }
    }
}