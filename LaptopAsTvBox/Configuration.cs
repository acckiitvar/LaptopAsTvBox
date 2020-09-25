using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LaptopAsTvBoxApp
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
        }

        private void LoadSettings(object sender, EventArgs e)
        {
            showMessageCheckBox.Checked = LaptopAsTvBoxApp.Properties.Settings.Default.ShowMessage;
        }

        private void SaveSettings(object sender, FormClosingEventArgs e)
        {
            // If the user clicked "Save"
            if (this.DialogResult == DialogResult.OK)
            {
                LaptopAsTvBoxApp.Properties.Settings.Default.ShowMessage = showMessageCheckBox.Checked;
                LaptopAsTvBoxApp.Properties.Settings.Default.Save();
            }
        }
    }
}