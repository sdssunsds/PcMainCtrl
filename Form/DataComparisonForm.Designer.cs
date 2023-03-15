
namespace PcMainCtrl.Form
{
    partial class DataComparisonForm
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cb_mode_sn = new System.Windows.Forms.ComboBox();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.cb_distance = new System.Windows.Forms.CheckBox();
            this.btn_distance = new System.Windows.Forms.Button();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.cb_mode_sn);
            this.flowLayoutPanel1.Controls.Add(this.cb_distance);
            this.flowLayoutPanel1.Controls.Add(this.btn_distance);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(800, 27);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // cb_mode_sn
            // 
            this.cb_mode_sn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_mode_sn.FormattingEnabled = true;
            this.cb_mode_sn.Location = new System.Drawing.Point(3, 3);
            this.cb_mode_sn.Name = "cb_mode_sn";
            this.cb_mode_sn.Size = new System.Drawing.Size(200, 20);
            this.cb_mode_sn.TabIndex = 0;
            this.cb_mode_sn.SelectedIndexChanged += new System.EventHandler(this.cb_mode_sn_SelectedIndexChanged);
            // 
            // dgv
            // 
            this.dgv.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6});
            this.dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv.Location = new System.Drawing.Point(0, 27);
            this.dgv.Name = "dgv";
            this.dgv.RowHeadersVisible = false;
            this.dgv.RowTemplate.Height = 23;
            this.dgv.Size = new System.Drawing.Size(800, 423);
            this.dgv.TabIndex = 1;
            // 
            // cb_distance
            // 
            this.cb_distance.AutoSize = true;
            this.cb_distance.Checked = true;
            this.cb_distance.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_distance.Location = new System.Drawing.Point(209, 3);
            this.cb_distance.Name = "cb_distance";
            this.cb_distance.Size = new System.Drawing.Size(96, 16);
            this.cb_distance.TabIndex = 1;
            this.cb_distance.Text = "比对位置数据";
            this.cb_distance.UseVisualStyleBackColor = true;
            // 
            // btn_distance
            // 
            this.btn_distance.Enabled = false;
            this.btn_distance.Location = new System.Drawing.Point(311, 3);
            this.btn_distance.Name = "btn_distance";
            this.btn_distance.Size = new System.Drawing.Size(200, 23);
            this.btn_distance.TabIndex = 2;
            this.btn_distance.Text = "回写方式：距离车头 <= 轴 + 偏轴";
            this.btn_distance.UseVisualStyleBackColor = true;
            this.btn_distance.Click += new System.EventHandler(this.btn_distance_Click);
            // 
            // Column1
            // 
            this.Column1.Frozen = true;
            this.Column1.HeaderText = "部件总编号";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 200;
            // 
            // Column2
            // 
            this.Column2.Frozen = true;
            this.Column2.HeaderText = "臂位";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 70;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "距离车头位置";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 120;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "轴位置";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            // 
            // Column5
            // 
            this.Column5.HeaderText = "偏离轴位置";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "数据偏差";
            this.Column6.Name = "Column6";
            this.Column6.ReadOnly = true;
            // 
            // DataComparisonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "DataComparisonForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "面阵数据比对";
            this.Load += new System.EventHandler(this.DataComparisonForm_Load);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ComboBox cb_mode_sn;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.CheckBox cb_distance;
        private System.Windows.Forms.Button btn_distance;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
    }
}