using IMusicSysDao;

namespace MusicSysDao.Oracle
{
    public partial class DataProvider : IDataProvider
    {
        protected static int commandtimeout = 0;
        protected static string dbtype = string.Empty;
        protected static string connectionstring = string.Empty;

        /// <summary>
        /// 获取连接数据库配置参数
        /// </summary>
        /// <param name="connectionstring">数据库连接字符串</param>
        /// <param name="dbtype">数据库类型</param>
        /// <param name="commandtimeout">获取或设置终止错误等待时间</param>
        public bool GetDbHelperConfig(string connectionString, string dbType, int commandTimeout)
        {
            connectionstring = connectionString;
            dbtype = dbType;
            commandtimeout = commandTimeout;

            return DbHelper.OpenConn();
        }
    }
}
