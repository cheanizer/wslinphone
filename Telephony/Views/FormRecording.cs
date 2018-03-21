using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LiteDB;
using Telephony.Models;
using Telephony.Helper;
using System.IO;

namespace Telephony.Views
{
    public partial class FormRecording : Form
    {
        private LiteDatabase db;
        private LiteCollection<Recording> collection;

        public FormRecording()
        {
            InitializeComponent();
            db = new LiteDatabase(@"nwrtelephony");
            collection = db.GetCollection<Recording>("recording");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Shift | Keys.D))
            {
                MessageBox.Show("are you sure to delete all?");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void fillTable()
        {
            dgvRecording.Rows.Clear();
            dgvRecording.Columns.Clear();
            dgvRecording.Columns.Add("filename", "Filename");
            dgvRecording.Columns.Add("location", "Location");
            dgvRecording.Columns.Add("start_at", "Start At");
            dgvRecording.Columns.Add("duration", "Duration");
            dgvRecording.Columns.Add("status", "Status");
            dgvRecording.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvRecording.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            var gridcontent = collection.FindAll();
            if (gridcontent != null)
            {
                foreach (var row in gridcontent)
                {
                    dgvRecording.Rows.Add(row.Filename, row.Location, row.StartAt.ToShortDateString(), row.Duration.ToString(), row.Status);
                }
            }
        }

        private void FormRecording_Load(object sender, EventArgs e)
        {
            fillTable();
        }

        private void dgvRecording_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != -1 && e.RowIndex != -1 && e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DataGridViewCell c = (sender as DataGridView)[e.ColumnIndex, e.RowIndex];
                if (!c.Selected)
                {
                    c.DataGridView.ClearSelection();
                    c.DataGridView.CurrentCell = c;
                    c.Selected = true;
                }
            }
        }

        private void dgvRecording_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu m = new ContextMenu();
                MenuItem playMenu = new MenuItem("Play");
                playMenu.Click += new System.EventHandler(this.playMenuClicked);
                m.MenuItems.Add(playMenu);
                MenuItem delMenu = new MenuItem("Delete");
                delMenu.Click += new System.EventHandler(this.deleteMenuClicked);
                m.MenuItems.Add(delMenu);
                m.Show(dgvRecording, new Point(e.X, e.Y));
            }
        }

        public void deleteMenuClicked(object sender, System.EventArgs e)
        {
            int index = dgvRecording.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dgvRecording.Rows[index];
            string location = Convert.ToString(selectedRow.Cells["location"].Value);
            string filename = Convert.ToString(selectedRow.Cells["filename"].Value);
            collection.Delete(i => i.Filename == filename);
            if (File.Exists(location))
            {
                try
                {
                    File.Delete(location);
                }catch(Exception err)
                {
                    MessageBox.Show(this,"File Not Found","File Not Found",MessageBoxButtons.OK);
                }
            }
            fillTable();
        }

        public void playMenuClicked(object sender, System.EventArgs e)
        {
            int index = dgvRecording.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dgvRecording.Rows[index];
            string location = Convert.ToString(selectedRow.Cells["location"].Value);
            string filename = Convert.ToString(selectedRow.Cells["filename"].Value);
            Form frmPlay = new Views.FormPlayback(location);
            frmPlay.Text = location;
            frmPlay.Owner = this;
            frmPlay.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DTMF dt = new DTMF();
            dt.setDtmf("9089602448961");
            dt.semua();
        }
    }
}
