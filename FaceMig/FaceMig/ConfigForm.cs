using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceMig
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NativeBridge.ModelReset();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var button = sender as CheckBox;
            if (button == null) return;
            if (button.Checked)
            {
                NativeBridge.ShowTrackingInfoWindow();
            }
            else
            {
                NativeBridge.CloseTrackingInfoWindow();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            var upDown = sender as NumericUpDown;
            if (upDown == null) return;
            NativeBridge.ReOpenDevice(Convert.ToInt32(upDown.Value));
        }
    }
}
