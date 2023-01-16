
namespace PcMainCtrl.Form
{
    partial class SaveRobotPointForm
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
            this.saveRobotPoint1 = new PcMainCtrl.Form.SaveRobotPoint();
            this.SuspendLayout();
            // 
            // saveRobotPoint1
            // 
            this.saveRobotPoint1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveRobotPoint1.Location = new System.Drawing.Point(0, 0);
            this.saveRobotPoint1.Name = "saveRobotPoint1";
            this.saveRobotPoint1.Size = new System.Drawing.Size(1006, 721);
            this.saveRobotPoint1.TabIndex = 0;
            // 
            // SaveRobotPointForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 721);
            this.Controls.Add(this.saveRobotPoint1);
            this.Name = "SaveRobotPointForm";
            this.Text = "SaveRobotPointForm";
            this.ResumeLayout(false);

        }

        #endregion

        private SaveRobotPoint saveRobotPoint1;
    }
}