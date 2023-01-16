namespace CognexLib
{
    partial class FormQuickBuild
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
            this.cogJobManagerEdit1 = new Cognex.VisionPro.QuickBuild.CogJobManagerEdit();
            this.SuspendLayout();
            // 
            // cogJobManagerEdit1
            // 
            this.cogJobManagerEdit1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cogJobManagerEdit1.AutoSize = true;
            this.cogJobManagerEdit1.Location = new System.Drawing.Point(16, 15);
            this.cogJobManagerEdit1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cogJobManagerEdit1.Name = "cogJobManagerEdit1";
            this.cogJobManagerEdit1.ShowLocalizationTab = false;
            this.cogJobManagerEdit1.Size = new System.Drawing.Size(831, 540);
            this.cogJobManagerEdit1.Subject = null;
            this.cogJobManagerEdit1.TabIndex = 0;
            // 
            // FormQuickBuild
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(848, 555);
            this.Controls.Add(this.cogJobManagerEdit1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormQuickBuild";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormQuickBuild";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormQuickBuild_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cognex.VisionPro.QuickBuild.CogJobManagerEdit cogJobManagerEdit1;
    }
}