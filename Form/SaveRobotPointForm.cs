using PcMainCtrl.HardWare;
using PcMainCtrl.HardWare.Robot;

namespace PcMainCtrl.Form
{
    public partial class SaveRobotPointForm : System.Windows.Forms.Form
    {
        public int HeadLocation
        {
            set { saveRobotPoint1.HeadLocation = value; }
        }
        public PLC3DCamera PLC3DCamera
        {
            set { saveRobotPoint1.PLC3DCamera = value; }
        }
        public LightManager Light
        {
            set { saveRobotPoint1.Light = value; }
        }

        public SaveRobotPointForm()
        {
            InitializeComponent();
        }

        public void RobotModInfoEvent(RobotGlobalInfo robotinfo, bool isFront)
        {
            saveRobotPoint1.RobotModInfoEvent(robotinfo, isFront);
        }
    }
}
