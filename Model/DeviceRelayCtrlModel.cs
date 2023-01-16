using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class DeviceRelayCtrlModel : NotifyBase
    {
        /// <summary>
        /// 协议信息
        /// </summary>
        private String relayRspMsg;
        public String RelayRspMsg
        {
            get { return relayRspMsg; }
            set
            {
                relayRspMsg = value;
                this.DoNotify();
            }
        }
    }
}
