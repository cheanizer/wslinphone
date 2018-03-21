using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telephony
{
    public partial class formSettingExt : Form
    {
        private Ippbx ippbx;
        public formSettingExt()
        {
            InitializeComponent();
        }
        
        internal Ippbx Ippbx { get => ippbx; set => ippbx = value; }

        private void button1_Click(object sender, EventArgs e)
        {
            saveSettingAndClose();
        }

        private void formSettingExt_Load(object sender, EventArgs e)
        {
            txtHost.Text = Properties.Settings.Default.pbx_hosts;
            txtExt.Text = Properties.Settings.Default.pbx_extension;
            txtPasswd.Text = Properties.Settings.Default.pbx_password;
            txtCallerId.Text = Properties.Settings.Default.pbx_caller;
        }

        public void saveSettingAndClose()
        {
            Properties.Settings.Default["pbx_hosts"] = txtHost.Text;
            Properties.Settings.Default["pbx_extension"] = txtExt.Text;
            Properties.Settings.Default["pbx_password"] = txtPasswd.Text;
            Properties.Settings.Default["pbx_caller"] = txtCallerId.Text;
            Properties.Settings.Default.Save();
            ippbx = new Ippbx();
            ippbx.hosts = txtHost.Text;
            ippbx.password = txtPasswd.Text;
            ippbx.extension = txtExt.Text;
            ippbx.callerid = txtExt.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtHost_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                saveSettingAndClose();
            }
        }

        private void txtExt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                saveSettingAndClose();
            }
        }

        private void txtPasswd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                saveSettingAndClose();
            }
        }
    }
}
