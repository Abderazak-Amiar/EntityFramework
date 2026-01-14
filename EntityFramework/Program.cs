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
                RegisterApplicationRestart(string.Empty, 0);
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

            // Apply EF migrations (removed legacy license checks per request)
            using (var context = new DataContext())
            {
                context.Database.Migrate();

                // Ensure a Parameters row exists (Id =1). If missing, create and persist it.
                var parameters = context.Parameters!.SingleOrDefault(p => p.Id == 1);
                if (parameters == null)
                {
                    parameters = new Parameters { Id = 1 };
                    context.Parameters!.Add(parameters);
                    context.SaveChanges();
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