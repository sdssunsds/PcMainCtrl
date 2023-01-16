using Cognex.VisionPro.QuickBuild;
using System.Windows.Forms;

namespace CognexLib
{
    public partial class FormQuickBuild : Form
    {
        public FormQuickBuild(CogJobManager mJobManager)
        {
            InitializeComponent();
            cogJobManagerEdit1.Subject = mJobManager;
        }

        private void FormQuickBuild_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogJobManagerEdit1.Dispose();
        }
    }
}
