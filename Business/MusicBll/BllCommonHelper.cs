using System.Data;

namespace MusicBll
{
    public class BllCommonHelper
    {
        /// <summary>
        /// 将DataSet转换为DataTable
        /// </summary>
        /// <param name="dSet"></param>
        /// <param name="i">表格ID</param>
        /// <returns></returns>
        public static DataTable ConvertDataTable(DataSet dSet, int i)
        {
            DataTable dTable = new DataTable();
            if (dSet != null && dSet.Tables[i] != null && dSet.Tables[i].Rows.Count > 0)
            {
                dTable = dSet.Tables[i];
            }
            return dTable;
        }
    }
}
