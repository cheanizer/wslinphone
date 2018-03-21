namespace Telephony.Views
{
    partial class FormRecording
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvRecording = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecording)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvRecording
            // 
            this.dgvRecording.AllowUserToAddRows = false;
            this.dgvRecording.AllowUserToDeleteRows = false;
            this.dgvRecording.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecording.Location = new System.Drawing.Point(0, 0);
            this.dgvRecording.Name = "dgvRecording";
            this.dgvRecording.ReadOnly = true;
            this.dgvRecording.Size = new System.Drawing.Size(856, 363);
            this.dgvRecording.TabIndex = 0;
            this.dgvRecording.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvRecording_CellMouseDown);
            this.dgvRecording.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvRecording_MouseClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 378);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Halaman";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(68, 375);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(37, 20);
            this.textBox1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 378);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "dari";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(131, 378);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(25, 13);
            this.lblTotal.TabIndex = 4;
            this.lblTotal.Text = "100";
            // 
            // FormRecording
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(856, 403);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvRecording);
            this.KeyPreview = true;
            this.Name = "FormRecording";
            this.ShowInTaskbar = false;
            this.Text = "FormRecording";
            this.Load += new System.EventHandler(this.FormRecording_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecording)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvRecording;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTotal;
    }
}