using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Management;

namespace LaptopAsTvBoxApp
{
    public class CheckDisplayTrayApp : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        Configuration configWindow = new Configuration();
        MenuItem MonitorCountMenuItem = new MenuItem("0 Monitors");

        public CheckDisplayTrayApp()
        {
            MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            
            SystemEvents.DisplaySettingsChanged += new EventHandler(DisplaySettingsChanged);

            notifyIcon.Icon = LaptopAsTvBoxApp.Properties.Resources.AppIcon;
            notifyIcon.DoubleClick += new EventHandler(ShowMessage);
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { MonitorCountMenuItem, configMenuItem, exitMenuItem });
            notifyIcon.Visible = true;

            DisplaySettingsChanged(null, null);
        }

        void ShowMessage(object sender, EventArgs e)
        {
            // Only show the message if the settings say we can.
            if (LaptopAsTvBoxApp.Properties.Settings.Default.ShowMessage)
                MessageBox.Show("Hello World");
        }

        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window meerly focus it.
            if (configWindow.Visible)
                configWindow.Focus();
            else
                configWindow.ShowDialog();
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;

            Application.Exit();
        }

        private int GetActiveDisplaysCount()
        {
            int numberOfMonitors = 0;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PnPEntity where service=\"monitor\"");

                numberOfMonitors = searcher.Get().Count;
            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
            return numberOfMonitors;
        }

        void DisplaySettingsChanged(object sender, EventArgs e)
        {
            int MonitorCount = GetActiveDisplaysCount();
            //MessageBox.Show("Display Events Got " + MonitorCount.ToString());
            
            if (MonitorCount > 1)
            {
                PowerScheme.PowerActiveSchemeSetLidAction(PowerScheme.LidAction.DoNothing);
                MonitorCountMenuItem.Text = MonitorCount.ToString() + " Monitors";
            } 
            else
            {
                PowerScheme.PowerActiveSchemeSetLidAction(PowerScheme.LidAction.Sleep);
                MonitorCountMenuItem.Text = MonitorCount.ToString() + " Monitor";
            }
        }
    }
}
