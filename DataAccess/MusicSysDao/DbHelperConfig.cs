using IMusicSysDao;
using MusicSysDao.Oracle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSysDao
{


    /// <summary>
    /// 数据库配置类
    /// </summary>
    public class DbHelperConfig : DataProvider
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string m_connectionstring { get { return DataProvider.connectionstring; } }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public static string m_dbtype { get { return DataProvider.dbtype; } }
        /// <summary>
        /// 获取或设置终止错误等待时间
        /// </summary>
        public static int m_commandtimeout { get { return DataProvider.commandtimeout; } }
    }
}
