using ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMusicSysDao
{
    public partial interface IDataProvider
    {
        /// <summary>
        /// 获取连接数据库配置参数
        /// </summary>
        /// <param name="connectionstring">数据库连接字符串</param>
        /// <param name="dbtype">数据库类型</param>
        /// <param name="commandtimeout">获取或设置终止错误等待时间</param>
        bool GetDbHelperConfig(string connectionstring, string dbtype, int commandtimeout);
    }
}
