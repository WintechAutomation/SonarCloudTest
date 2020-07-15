namespace SonarCloudTest
{
    partial class Multiline
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
            this.tbMultiLine = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbMultiLine
            // 
            this.tbMultiLine.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMultiLine.BackColor = System.Drawing.Color.DimGray;
            this.tbMultiLine.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tbMultiLine.ForeColor = System.Drawing.Color.White;
            this.tbMultiLine.Location = new System.Drawing.Point(0, 0);
            this.tbMultiLine.MaxLength = 4;
            this.tbMultiLine.Multiline = true;
            this.tbMultiLine.Name = "tbMultiLine";
            this.tbMultiLine.Size = new System.Drawing.Size(223, 202);
            this.tbMultiLine.TabIndex = 31;
            this.tbMultiLine.Tag = "";
            this.tbMultiLine.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tbMultiLine_MouseDoubleClick);
            // 
            // Multiline
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 202);
            this.Controls.Add(this.tbMultiLine);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Multiline";
            this.Text = "Multiline";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox tbMultiLine;

    }
}