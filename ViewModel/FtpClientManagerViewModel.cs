using PcMainCtrl.Common;
using PcMainCtrl.FtpClient;
using PcMainCtrl.HardWare.BaslerCamera;
using PcMainCtrl.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using static PcMainCtrl.Common.ThreadManager;

namespace PcMainCtrl.ViewModel
{
    public class FtpClientManagerViewModel : NotifyBase
    {
        /// <summary>
        /// FtpServer信息
        /// </summary>
        public FtpClientManagerModel FtpServerInfo { get; set; } = new FtpClientManagerModel();

        /// <summary>
        /// 左侧指定目录文件列表
        /// </summary>
        public ObservableCollection<String> RemoteFileList { get; set; } = new ObservableCollection<String>();
        
        /// <summary>
        /// 右侧指定目录文件列表 
        /// </summary>
        public ObservableCollection<String> LocalFileList { get; set; } = new ObservableCollection<String>();

        /// <summary>
        /// Ftp登录
        /// </summary>
        public CommandBase FtpLoginCmd { get; set; }

        /// <summary>
        /// Ftp退出
        /// </summary>
        public CommandBase FtpQuitCmd { get; set; }

        /// <summary>
        /// Ftp刷新
        /// </summary>
        public CommandBase FtpRefreshCmd { get; set; }

        /// <summary>
        /// Ftp直接上传文件
        /// </summary>
        public CommandBase FtpDirectUploadCmd { get; set; }

        /// <summary>
        /// Ftp压缩后上传文件
        /// </summary>
        public CommandBase FtpZipUploadCmd { get; set; }

        public FtpClientManagerViewModel()
        {
            this.FtpLoginCmd = new CommandBase();
            this.FtpLoginCmd.DoExecute = new Action<object>(DoFtpLoginCmdHandle);
            this.FtpLoginCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FtpQuitCmd = new CommandBase();
            this.FtpQuitCmd.DoExecute = new Action<object>(DoFtpQuitCmdHandle);
            this.FtpQuitCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FtpRefreshCmd = new CommandBase();
            this.FtpRefreshCmd.DoExecute = new Action<object>(DoFtpRefreshCmdHandle);
            this.FtpRefreshCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FtpDirectUploadCmd = new CommandBase();
            this.FtpDirectUploadCmd.DoExecute = new Action<object>(DoFtpDirectUploadCmdHandle);
            this.FtpDirectUploadCmd.DoCanExecute = new Func<object, bool>((o) => true);

            this.FtpZipUploadCmd = new CommandBase();
            this.FtpZipUploadCmd.DoExecute = new Action<object>(DoFtpZipUploadCmdHandle);
            this.FtpZipUploadCmd.DoCanExecute = new Func<object, bool>((o) => true);

            FtpServerInfo.FtpServerIp = Properties.Settings.Default.FtpIP;
            FtpServerInfo.FtpServerPort = Properties.Settings.Default.FtpPort;
            FtpServerInfo.FtpServerUser = Properties.Settings.Default.FtpUser;
            FtpServerInfo.FtpServerPasswd = Properties.Settings.Default.FtpPwd;

            InitLocalFileListView(System.Environment.CurrentDirectory + @"\task_data\xz_camera\");
        }

        /// <summary>
        /// 获得指定路径下所有文件名
        /// StreamWriter sw  文件写入流
        /// string path      文件路径
        /// </summary>
        private void InitLocalFileListView(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] inf = dir.GetFiles();
            foreach (FileInfo finf in inf)
            {
                //如果扩展名为“.bmp”
                if (finf.Extension.Equals(".jpg"))
                {
                    //读取文件的完整目录和文件名
                    LocalFileList.Add(finf.FullName);
                }
            }
        }

        /// <summary>
        /// 判断输入信息
        /// </summary>
        /// <returns></returns>
        private bool CheckInfo()
        {
            string ipAddr = FtpServerInfo.FtpServerIp;
            string port = FtpServerInfo.FtpServerPort;
            string userName = FtpServerInfo.FtpServerUser;
            string password = FtpServerInfo.FtpServerPasswd;

            if (string.IsNullOrEmpty(ipAddr) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                System.Windows.Forms.MessageBox.Show("请输入登录信息");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 列出目录
        /// </summary>
        private void ListDirectory()
        {
            bool isOk = false;

            string ipAddr = FtpServerInfo.FtpServerIp;
            string port = FtpServerInfo.FtpServerPort;
            string userName = FtpServerInfo.FtpServerUser;
            string password = FtpServerInfo.FtpServerPasswd;
            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);

            string[] arrAccept = ftpHelper.ListDirectory(out isOk);//调用Ftp目录显示功能
            if (isOk)
            {
                RemoteFileList.Clear();
                foreach (string accept in arrAccept)
                {
                    string name = accept.Substring(39);
                    if (accept.IndexOf("<DIR>") != -1)
                    {
                        //文件夹
                    }
                    else
                    {
                        //文件
                        RemoteFileList.Add(name);
                    }
                }

                FtpServerInfo.FtpServeMsg = "列出目录成功";
            }
            else
            {
                ftpHelper.SetPrePath();
                FtpServerInfo.FtpServeMsg = "链接失败，或者没有数据";
            }
        }

        /// <summary>
        /// Ftp登录
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpLoginCmdHandle(object obj)
        {
            try
            {
                if (CheckInfo())
                {
                    ListDirectory();
                }
            }
            catch (Exception exception)
            {
                System.Windows.Forms.MessageBox.Show("Exception caught:\n" + exception.Message);
            }
        }

        /// <summary>
        /// Ftp退出
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpQuitCmdHandle(object obj)
        {
            System.Windows.Forms.Application.Exit();
        }

        /// <summary>
        /// Ftp刷新
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpRefreshCmdHandle(object obj)
        {
            ListDirectory();
        }

        /// <summary>
        /// Ftp直接上传
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpDirectUploadCmdHandle(object obj)
        {
            String path = obj as String;

            string ipAddr = FtpServerInfo.FtpServerIp;
            string port = FtpServerInfo.FtpServerPort;
            string userName = FtpServerInfo.FtpServerUser;
            string password = FtpServerInfo.FtpServerPasswd;
            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);

            if (File.Exists(path))
            {
                ftpHelper.RelatePath = string.Format("{0}/{1}", ftpHelper.RelatePath, Path.GetFileName(path));

                bool isOk = false;
                ftpHelper.UpLoad(path, out isOk);
                ftpHelper.SetPrePath();
                if (isOk)
                {
                    this.ListDirectory();
                    FtpServerInfo.FtpServeMsg = "上传成功";
                }
                else
                {
                    FtpServerInfo.FtpServeMsg = "上传失败";
                }
            }
        }

        /// <summary>
        /// 线阵压缩
        /// </summary>
        /// <param name="obj"></param>
        void DoCameraCmdHandle_Compress(object obj)
        {
            String name = obj as String;

            List<Image> img_list = new List<Image>();
            img_list.Clear();

            TaskRun(new Action(() =>
            {
                //1.首先读取本地的所有图片文件
                String src_dir = System.Environment.CurrentDirectory + @"\task_data\xz_camera\";
                String dest_dir = System.Environment.CurrentDirectory + @"\task_data\xz_compress\" + name + ".zip";

                CameraDataHelper.CreateZipFile(src_dir, dest_dir);
            }));
        }

        /// <summary>
        /// Ftp压缩后上传
        /// </summary>
        /// <param name="obj"></param>
        private void DoFtpZipUploadCmdHandle(object obj)
        {
            //压缩打包
            //String path = obj as String;
            String path = @"CH380AL-123456";
            DoCameraCmdHandle_Compress(path);

            //找到压缩文件
            String dest_path= System.Environment.CurrentDirectory + @"\task_data\xz_compress\" + path + ".zip";

            //上传文件
            string ipAddr = FtpServerInfo.FtpServerIp;
            string port = FtpServerInfo.FtpServerPort;
            string userName = FtpServerInfo.FtpServerUser;
            string password = FtpServerInfo.FtpServerPasswd;
            FtpHelper ftpHelper = FtpHelper.GetInstance(ipAddr, port, userName, password);

            if (File.Exists(dest_path))
            {
                ftpHelper.RelatePath = string.Format("{0}/{1}", ftpHelper.RelatePath, Path.GetFileName(dest_path));

                bool isOk = false;
                ftpHelper.UpLoad(dest_path, out isOk);
                ftpHelper.SetPrePath();
                if (isOk)
                {
                    this.ListDirectory();
                    FtpServerInfo.FtpServeMsg = "上传成功";
                }
                else
                {
                    FtpServerInfo.FtpServeMsg = "上传失败";
                }
            }
        }
    }
}
