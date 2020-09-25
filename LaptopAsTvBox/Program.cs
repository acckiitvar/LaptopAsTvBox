using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace LaptopAsTvBoxApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                MessageBox.Show("Application already running. \nOnly one instance of this application is allowed.");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Instead of running a form, we run an ApplicationContext.
            Application.Run(new CheckDisplayTrayApp());
        }
    }
}