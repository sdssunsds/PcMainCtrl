using System;

namespace PcMainCtrl.Form
{
    public partial class InputForm : System.Windows.Forms.Form
    {
        public int HeadLocation { get; set; }

        public InputForm()
        {
            InitializeComponent();
        }

        private void InputForm_Shown(object sender, EventArgs e)
        {
            textBox1.Text = HeadLocation.ToString();
            textBox1.SelectAll();
        }

        private void InputForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (int.TryParse(textBox1.Text, out _))
            {
                HeadLocation = int.Parse(textBox1.Text);
            }
        }
    }
}
