using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wrec
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => LogError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => LogError((Exception)e.ExceptionObject);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Erreur critique : {ex.Message}", "SR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void LogError(Exception ex)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {ex.ToString()}\n\n");
        }
    }
}
