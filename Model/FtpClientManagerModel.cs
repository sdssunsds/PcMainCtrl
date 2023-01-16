using PcMainCtrl.Common;
using System;

namespace PcMainCtrl.Model
{
    public class FtpClientManagerModel : NotifyBase
    {
        /// <summary>
        /// FtpServerIp
        /// </summary>
        private String ftpServerIp;

        public String FtpServerIp
        {
            get { return ftpServerIp; }
            set { ftpServerIp = value; }
        }

        /// <summary>
        /// FtpServerPort
        /// </summary>
        private String ftpServerPort;

        public String FtpServerPort
        {
            get { return ftpServerPort; }
            set { ftpServerPort = value; }
        }

        /// <summary>
        /// FtpServerUser
        /// </summary>
        private String ftpServerUser;

        public String FtpServerUser
        {
            get { return ftpServerUser; }
            set { ftpServerUser = value; }
        }

        /// <summary>
        /// FtpServerPasswd
        /// </summary>
        private String ftpServerPasswd;

        public String FtpServerPasswd
        {
            get { return ftpServerPasswd; }
            set { ftpServerPasswd = value; }
        }

        /// <summary>
        /// FtpServeMsg
        /// </summary>
        private String ftpServeMsg;

        public String FtpServeMsg
        {
            get { return ftpServeMsg; }
            set { ftpServeMsg = value; }
        }
    } 
}
