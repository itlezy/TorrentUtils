
namespace ILMagnetToURI
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnParse = new System.Windows.Forms.Button();
            this.txtURLs = new System.Windows.Forms.TextBox();
            this.txtHashIds = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.timerClipMonitor = new System.Windows.Forms.Timer(this.components);
            this.ckAutoProcess = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnParse
            // 
            this.btnParse.Location = new System.Drawing.Point(9, 3);
            this.btnParse.Name = "btnParse";
            this.btnParse.Size = new System.Drawing.Size(75, 23);
            this.btnParse.TabIndex = 0;
            this.btnParse.Text = "Pars&e";
            this.btnParse.UseVisualStyleBackColor = true;
            this.btnParse.Click += new System.EventHandler(this.btnParse_Click);
            // 
            // txtURLs
            // 
            this.txtURLs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtURLs.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtURLs.Location = new System.Drawing.Point(3, 3);
            this.txtURLs.Multiline = true;
            this.txtURLs.Name = "txtURLs";
            this.txtURLs.Size = new System.Drawing.Size(626, 449);
            this.txtURLs.TabIndex = 1;
            this.txtURLs.WordWrap = false;
            // 
            // txtHashIds
            // 
            this.txtHashIds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtHashIds.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtHashIds.Location = new System.Drawing.Point(635, 3);
            this.txtHashIds.Multiline = true;
            this.txtHashIds.Name = "txtHashIds";
            this.txtHashIds.Size = new System.Drawing.Size(626, 449);
            this.txtHashIds.TabIndex = 2;
            this.txtHashIds.WordWrap = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.txtURLs, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtHashIds, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 561);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ckAutoProcess);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.btnClear);
            this.panel1.Controls.Add(this.btnOpen);
            this.panel1.Controls.Add(this.btnParse);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 458);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(626, 100);
            this.panel1.TabIndex = 3;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(252, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(171, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clea&r";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(90, 3);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Ope&n";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // timerClipMonitor
            // 
            this.timerClipMonitor.Enabled = true;
            this.timerClipMonitor.Interval = 999;
            this.timerClipMonitor.Tick += new System.EventHandler(this.timerClipMonitor_Tick);
            // 
            // ckAutoProcess
            // 
            this.ckAutoProcess.AutoSize = true;
            this.ckAutoProcess.Checked = true;
            this.ckAutoProcess.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckAutoProcess.Location = new System.Drawing.Point(9, 33);
            this.ckAutoProcess.Name = "ckAutoProcess";
            this.ckAutoProcess.Size = new System.Drawing.Size(89, 17);
            this.ckAutoProcess.TabIndex = 4;
            this.ckAutoProcess.Text = "A&uto Process";
            this.ckAutoProcess.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 561);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnParse;
        private System.Windows.Forms.TextBox txtURLs;
        private System.Windows.Forms.TextBox txtHashIds;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Timer timerClipMonitor;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.CheckBox ckAutoProcess;
    }
}

