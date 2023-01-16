using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class DeviceCameraCtrlModel : NotifyBase
    {
        /// <summary>
        /// Basler camera选择
        /// </summary>
        //private ICameraInfo camerainfoItem;
        //public ICameraInfo CamerainfoItem
        //{
        //    get { return camerainfoItem; }
        //    set 
        //    { 
        //        camerainfoItem = value; 
        //        this.DoNotify(); 
        //    }
        //}

        /// <summary>
        /// Basler camera的Tag信息
        /// </summary>
        private String cameraFriendlyName;

        public String CameraFriendlyName
        {
            get { return cameraFriendlyName; }
            set 
            { 
                cameraFriendlyName = value;
                this.DoNotify();
            }
        }
    }
}
