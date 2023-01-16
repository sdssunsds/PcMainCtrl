using PcMainCtrl.DataAccess.DataEntity;

namespace PcMainCtrl.Common
{
    /// <summary>
    /// 管理的全局状态信息
    /// </summary>
    public class GlobalValues
    {
        public static UserEntity UserInfo { get; set; } = new UserEntity();

        public static event AddLogs AddLogEvent;

        public static void AddLog(string log, int type = 0)
        {
            AddLogEvent?.Invoke(log, type);
        }
    }
    public delegate void AddLogs(string log, int type);
}
