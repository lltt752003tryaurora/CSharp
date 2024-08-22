namespace QuestionProcessorFormApp
{
    partial class frmMain
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
            this.btnStatus = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.btnChooseFile = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.btnWord2MathML = new System.Windows.Forms.Button();
            this.btnTable2Image = new System.Windows.Forms.Button();
            this.parseHtmlFile = new System.Windows.Forms.Button();
            this.btnParseV2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnW2ML = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStatus
            // 
            this.btnStatus.Location = new System.Drawing.Point(22, 41);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(146, 33);
            this.btnStatus.TabIndex = 0;
            this.btnStatus.Text = "Start";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.btnStatus_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 216);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(484, 288);
            this.txtLog.TabIndex = 1;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(16, 127);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(261, 20);
            this.txtFileName.TabIndex = 2;
            // 
            // btnChooseFile
            // 
            this.btnChooseFile.Location = new System.Drawing.Point(163, 148);
            this.btnChooseFile.Name = "btnChooseFile";
            this.btnChooseFile.Size = new System.Drawing.Size(114, 33);
            this.btnChooseFile.TabIndex = 3;
            this.btnChooseFile.Text = "Chọn file";
            this.btnChooseFile.UseVisualStyleBackColor = true;
            this.btnChooseFile.Click += new System.EventHandler(this.btnChooseFile_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(64, 20);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(100, 33);
            this.btnConvert.TabIndex = 4;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // btnWord2MathML
            // 
            this.btnWord2MathML.Location = new System.Drawing.Point(214, 71);
            this.btnWord2MathML.Name = "btnWord2MathML";
            this.btnWord2MathML.Size = new System.Drawing.Size(228, 34);
            this.btnWord2MathML.TabIndex = 5;
            this.btnWord2MathML.Text = "Word2MathML";
            this.btnWord2MathML.UseVisualStyleBackColor = true;
            this.btnWord2MathML.Click += new System.EventHandler(this.btnWord2MathML_Click);
            // 
            // btnTable2Image
            // 
            this.btnTable2Image.Location = new System.Drawing.Point(182, 19);
            this.btnTable2Image.Name = "btnTable2Image";
            this.btnTable2Image.Size = new System.Drawing.Size(114, 34);
            this.btnTable2Image.TabIndex = 6;
            this.btnTable2Image.Text = "Table2Image";
            this.btnTable2Image.UseVisualStyleBackColor = true;
            this.btnTable2Image.Click += new System.EventHandler(this.btnTable2Image_Click);
            // 
            // parseHtmlFile
            // 
            this.parseHtmlFile.Location = new System.Drawing.Point(302, 19);
            this.parseHtmlFile.Name = "parseHtmlFile";
            this.parseHtmlFile.Size = new System.Drawing.Size(140, 33);
            this.parseHtmlFile.TabIndex = 7;
            this.parseHtmlFile.Text = "Parse htm file";
            this.parseHtmlFile.UseVisualStyleBackColor = true;
            this.parseHtmlFile.Click += new System.EventHandler(this.parseHtmlFile_Click);
            // 
            // btnParseV2
            // 
            this.btnParseV2.Location = new System.Drawing.Point(40, 76);
            this.btnParseV2.Name = "btnParseV2";
            this.btnParseV2.Size = new System.Drawing.Size(145, 29);
            this.btnParseV2.TabIndex = 8;
            this.btnParseV2.Text = "Parse v2";
            this.btnParseV2.UseVisualStyleBackColor = true;
            this.btnParseV2.Click += new System.EventHandler(this.btnParseV2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnW2ML);
            this.groupBox1.Controls.Add(this.parseHtmlFile);
            this.groupBox1.Controls.Add(this.txtFileName);
            this.groupBox1.Controls.Add(this.btnChooseFile);
            this.groupBox1.Controls.Add(this.btnParseV2);
            this.groupBox1.Controls.Add(this.btnTable2Image);
            this.groupBox1.Controls.Add(this.btnConvert);
            this.groupBox1.Controls.Add(this.btnWord2MathML);
            this.groupBox1.Location = new System.Drawing.Point(219, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(457, 181);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "test";
            this.groupBox1.Visible = false;
            // 
            // btnW2ML
            // 
            this.btnW2ML.Location = new System.Drawing.Point(302, 123);
            this.btnW2ML.Name = "btnW2ML";
            this.btnW2ML.Size = new System.Drawing.Size(140, 23);
            this.btnW2ML.TabIndex = 9;
            this.btnW2ML.Text = "Convert Word";
            this.btnW2ML.UseVisualStyleBackColor = true;
            this.btnW2ML.Click += new System.EventHandler(this.btnW2ML_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(531, 216);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(35, 13);
            this.lblStatus.TabIndex = 10;
            this.lblStatus.Text = "label1";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 153);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(146, 24);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 535);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnStatus);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmMain";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Button btnChooseFile;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnWord2MathML;
        private System.Windows.Forms.Button btnTable2Image;
        private System.Windows.Forms.Button parseHtmlFile;
        private System.Windows.Forms.Button btnParseV2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnW2ML;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button button1;
    }
}

