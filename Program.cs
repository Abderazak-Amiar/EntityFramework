using System;
using System.IO;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        string appName = "GestionHuilerie";
        string userFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
        Directory.CreateDirectory(userFolder);

        // Make |DataDirectory| point to a user-writable folder
        AppDomain.CurrentDomain.SetData("DataDirectory", userFolder);

        // Copy DB shipped in the app folder to the user folder on first run
        string shippedDb = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData.db"); // adjust name
        string targetDb = Path.Combine(userFolder, "UserData.db");
        if (!File.Exists(targetDb) && File.Exists(shippedDb))
        {
            File.Copy(shippedDb, targetDb);
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}