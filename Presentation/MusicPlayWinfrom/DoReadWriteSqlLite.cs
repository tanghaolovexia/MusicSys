using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MusicBll;
using System.IO;
using ViewModel;

namespace MusicPlayWinfrom
{
    public class DoReadWriteSqlLite : ConnctionSqlLite
    {
        private static string m_sqllitepath = GetSqlLitePath();
        public DoReadWriteSqlLite()
            : base(m_sqllitepath)
        { }

        /// <summary>
        /// 获取本地数据库路径
        /// </summary>
        /// <returns></returns>
        private static string GetSqlLitePath()
        {
            string result = System.AppDomain.CurrentDomain.BaseDirectory;
            result = string.Concat(result.Substring(0, result.IndexOf("bin")), @"data\localmusiclist.db");
            //判断sqllite文件是否存在,如果不存在则直接创建
            if (!File.Exists(result))
            {
                File.Create(result);
            }
            return result;
        }

        public bool InsertLocalMusicList(List<tb_SaveMusicList> list)
        {
            return ConnctionSqlLite.InsertLocalMusicList(list);
        }
    }
}
