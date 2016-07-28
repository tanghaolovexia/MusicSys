using MusicSysDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBll
{
    /// <summary>
    /// 连接SqlLite数据库
    /// </summary>
    public class CommonConnctionSqlLite
    {
        /// <summary>
        /// SqlLite数据库物理地址
        /// </summary>
        private static string m_sqllitepath = string.Empty;
        /// <summary>
        /// SqlLite连接字符串
        /// </summary>
        private static string m_connctionsql = string.Empty;

        public CommonConnctionSqlLite()
        {
        }

        private static string GetConnctionSql()
        {
            return string.Format("Data Source={0};Version=3;Pooling=False;Max Pool Size=100;", m_sqllitepath);
        }

        /// <summary>
        /// 返回SqlLite构造参数
        /// </summary>
        /// <returns></returns>
        public static Data_SqlLiteHelper GetSqlLiteHelper(string sqllitepath)
        {
            m_sqllitepath = sqllitepath;
            m_connctionsql = GetConnctionSql();
            Data_SqlLiteHelper helper = new Data_SqlLiteHelper(m_sqllitepath, m_connctionsql);
            return helper;
        }
    }
}
