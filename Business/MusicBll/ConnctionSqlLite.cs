using MusicSysDao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModel;

namespace MusicBll
{
    public class ConnctionSqlLite : IConnctionSqlLite
    {
        /// <summary>
        /// 数据库连接类
        /// </summary>
        private static Data_SqlLiteHelper m_dataSqlLiteHelper = null;
        /// <summary>
        /// 数据库本地地址
        /// </summary>
        private static string m_sqllitepath = string.Empty;
        /// <summary>
        /// 等待执行的SqlLite语句
        /// </summary>
        private static StringBuilder m_runsql = new StringBuilder();

        public ConnctionSqlLite(string sqllitepath)
        {
            m_sqllitepath = sqllitepath;
            GetSqlLiteHelper();
        }

        public Data_SqlLiteHelper GetSqlLiteHelper()
        {
            if (m_dataSqlLiteHelper == null)
            {
                m_dataSqlLiteHelper = CommonConnctionSqlLite.GetSqlLiteHelper(m_sqllitepath);
            }
            return m_dataSqlLiteHelper;
        }

        /// <summary>
        /// 将本地音乐信息添加进我们数据库
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool InsertLocalMusicList(List<tb_SaveMusicList> list)
        {
            bool result = false;
            m_runsql = new StringBuilder();

            try
            {
                foreach (var music in list)
                {
                    m_runsql.AppendFormat("insert into tb_SaveMusicList values({0},'{1}','{2}','{3}');", music.ID, music.Name, music.SingLength, music.Singer);
                }
                int count = (int)m_dataSqlLiteHelper.ExecuteScalar(m_runsql.ToString());
                if (count > 0)
                {
                    result = true;
                }
            }
            catch (Exception) { result = false; }
            return result;
        }
        /// <summary>
        /// 根据表名获取表自增ID
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static int GetTableSequence(string tablename)
        {
            //自增ID
            int maxID = 0;
            List<sqlite_sequence> seqs = new List<sqlite_sequence>();
            m_runsql = new StringBuilder();
            try
            {
                m_runsql.AppendFormat("select name,seq  from sqlite_sequence where name = '{0}'", tablename);
                DataSet dset = m_dataSqlLiteHelper.GetDs(m_runsql.ToString());
                DataTable dtable = BllCommonHelper.ConvertDataTable(dset, 0);
                if (dtable != null && dtable.Rows.Count > 0)
                {
                    seqs = AssemblyEntity<sqlite_sequence>.SetEntitys(dtable);
                    foreach (var s in seqs)
                    {
                        maxID = s.seq;
                    }
                }
            }
            catch (Exception) { }
            return maxID;
        }
    }
}
