using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace MusicSysDao
{
    public class SqlServerProvider
    {
        public DbProviderFactory Instance()
        {
            return SqlClientFactory.Instance;
        }

        public void DeriveParameters(IDbCommand cmd)
        {
            if ((cmd as SqlCommand) != null)
            {
                SqlCommandBuilder.DeriveParameters(cmd as SqlCommand);
            }
        }

        public DbParameter MakeParam(string ParamName, DbType DbType, Int32 Size)
        {
            SqlParameter param;
            if (Size > 0)
                param = new SqlParameter(ParamName, (SqlDbType)DbType, Size);
            else
                param = new SqlParameter(ParamName, (SqlDbType)DbType);
            return param;
        }

        public bool IsFullTextSearchEnabled()
        {
            return true;
        }

        public bool IsCompactDatabase()
        {
            return true;
        }

        public bool IsBackupDatabase()
        {
            return true;
        }

        public string GetLastIdSql()
        {
            return "SELECT @@IDENTITY";
        }

        public bool IsDbOptimize()
        {
            return false;
        }

        public bool IsShrinkData()
        {
            return true;
        }

        public bool IsStoreProc()
        {
            return true;
        }
    }
}
