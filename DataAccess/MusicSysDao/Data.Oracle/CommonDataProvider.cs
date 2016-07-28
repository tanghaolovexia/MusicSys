using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace MusicSysDao.Oracle
{
    /// <summary>
    /// 用于生成公共的Provider部分
    /// </summary>
    public partial class DataProvider
    {
        /// <summary>
        /// 全局变量用于存储MySql执行语句
        /// </summary>
        private string runtmysql = string.Empty;
        /// <summary>
        /// 全局变量用于拼接MYSQL条件语句
        /// </summary>
        private string whereSql = string.Empty;
        /// <summary>
        /// 获取DbParameter参数
        /// </summary>
        private List<DbParameter> AddListDbParameter = new List<DbParameter>();
        /// <summary>
        /// 全局变量方法是否成功
        /// </summary>
        private bool flag = false;

        /// <summary>
        /// 将DbParameter List转换为数组类型
        /// </summary>
        /// <param name="listparameter"></param>
        /// <returns></returns>
        private DbParameter[] ConvertDataParameter(List<DbParameter> listparameter)
        {
            DbParameter[] param = new DbParameter[] { };
            int i = 0;
            if (listparameter != null && listparameter.Count > 0)
            {
                foreach (var p in listparameter)
                {
                    param[i] = p;
                    i++;
                }
            }
            return param;
        }

        /// <summary>
        /// 将DataSet转换为DataTable
        /// </summary>
        /// <param name="dSet"></param>
        /// <returns></returns>
        private DataTable ConvertDataTable(DataSet dSet)
        {
            DataTable dTable = new DataTable();
            if (dSet != null && dSet.Tables[0] != null && dSet.Tables[0].Rows.Count > 0)
            {
                dTable = dSet.Tables[0];
            }
            return dTable;
        }
        /// <summary>
        /// 将DataSet转换为DataTable
        /// </summary>
        /// <param name="dSet"></param>
        /// <param name="i">表格ID</param>
        /// <returns></returns>
        private DataTable ConvertDataTable(DataSet dSet, int i)
        {
            DataTable dTable = new DataTable();
            if (dSet != null && dSet.Tables[i] != null && dSet.Tables[i].Rows.Count > 0)
            {
                dTable = dSet.Tables[i];
            }
            return dTable;
        }

        private DataTable ConvertDataTable1(DataSet dSet)
        {
            DataTable dTable = new DataTable();
            if (dSet != null && dSet.Tables[1] != null && dSet.Tables[1].Rows.Count > 0)
            {
                dTable = dSet.Tables[1];
            }
            return dTable;
        }
        /// <summary>
        /// 分页获取数据列表参数
        /// </summary>
        /// <param name="_pagecurrent">当前页</param>
        /// <param name="_pagesize">每页的记录数</param>
        /// <param name="_ifelse">显示字段</param>
        /// <param name="_where">条件</param>
        /// <param name="_order">排序</param>
        /// <returns></returns>
        private DbParameter[] GetListPadingParameter(int _pagecurrent, int _pagesize, string _ifelse, string _where, string _order)
        {
            //DbParameter[] param = { 
            //                      DbHelper.MakeMySqlInParam("?_pagecurrent",(DbType)MySqlDbType.Int32,5,_pagecurrent),
            //                      DbHelper.MakeMySqlInParam("?_pagesize",(DbType)MySqlDbType.Int32,5,_pagesize),
            //                      DbHelper.MakeMySqlInParam("?_ifelse",(DbType)MySqlDbType.VarChar,200,_ifelse),
            //                      DbHelper.MakeMySqlInParam("?_where",(DbType)MySqlDbType.VarChar,500,_where),
            //                      DbHelper.MakeMySqlInParam("?_order",(DbType)MySqlDbType.VarChar,100,_order)
            //                      };
            return null;
        }
        /// <summary>
        /// 分页获取数据列表参数
        /// </summary>
        /// <param name="_pagecurrent">当前页</param>
        /// <param name="_pagesize">每页的记录数</param>
        /// <param name="_ifelse">显示字段</param>
        /// <param name="_where">条件</param>
        /// <param name="_order">排序</param>
        /// <param name="_totalfield">总数filed</param>
        /// <returns></returns>
        private DbParameter[] GetListPadingParameterTwo(int _pagecurrent, int _pagesize, string _ifelse, string _where, string _order, string _totalfield)
        {
            //DbParameter[] param = { 
            //                      DbHelper.MakeMySqlInParam("?_pagecurrent",(DbType)MySqlDbType.Int32,5,_pagecurrent),
            //                      DbHelper.MakeMySqlInParam("?_pagesize",(DbType)MySqlDbType.Int32,5,_pagesize),
            //                      DbHelper.MakeMySqlInParam("?_ifelse",(DbType)MySqlDbType.VarChar,200,_ifelse),
            //                      DbHelper.MakeMySqlInParam("?_where",(DbType)MySqlDbType.VarChar,500,_where),
            //                      DbHelper.MakeMySqlInParam("?_order",(DbType)MySqlDbType.VarChar,100,_order),
            //                      DbHelper.MakeMySqlInParam("?_totalfield",(DbType)MySqlDbType.VarChar,20,_totalfield)
            //                      };
            return null;
        }
    }
}
