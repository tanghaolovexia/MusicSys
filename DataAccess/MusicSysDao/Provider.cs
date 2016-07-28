using IMusicSysDao;
using ViewModel;
using System;

namespace MusicSysDao
{
    public class Provider
    {
        #region var
        /// <summary>
        /// 锁对象
        /// </summary>
        private static object lockHelper = new object();

        /// <summary>
        /// 获取数据访问接口
        /// </summary>
        private static IDataProvider _Instance = null;
        /// <summary>
        /// 获取数据访问接口
        /// </summary>
        public static IDataProvider SqlServerInstance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockHelper)
                    {
                        if (_Instance == null)
                        {
                            GetProvider((int)DbEnumType.SqlServer);
                        }
                    }
                }
                return _Instance;
            }
        }

        public static IDataProvider MySqlInstance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockHelper)
                    {
                        if (_Instance == null)
                        {
                            GetProvider((int)DbEnumType.MySql);
                        }
                    }
                }
                return _Instance;
            }
        }

        public static IDataProvider OracleInstance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (lockHelper)
                    {
                        if (_Instance == null)
                        {
                            GetProvider((int)DbEnumType.Oracle);
                        }
                    }
                }
                return _Instance;
            }
        }
        #endregion var

        #region constructor
        static Provider()
        {
            GetProvider((int)DbEnumType.SqlServer);
        }

        private static void GetProvider(int dbtype)
        {
            try
            {
                switch (dbtype)
                {
                    case (int)DbEnumType.SqlServer:
                        _Instance = (IDataProvider)Activator.CreateInstance(Type.GetType(string.Format("SecordShow.DataSourceDao.{0}.DataProvider", DbEnumType.SqlServer.ToString()), false, true));
                        break;
                    case (int)DbEnumType.MySql:
                        _Instance = (IDataProvider)Activator.CreateInstance(Type.GetType(string.Format("SecordShow.DataSourceDao.{0}.DataProvider", DbEnumType.MySql.ToString()), false, true));
                        break;
                    case (int)DbEnumType.Oracle:
                        _Instance = (IDataProvider)Activator.CreateInstance(Type.GetType(string.Format("SecordShow.DataSourceDao.{0}.DataProvider", DbEnumType.Oracle.ToString()), false, true));
                        break;
                }               
            }
            catch
            {
                //LogBLL.error(PageInfo + "方法名：GetProvider\t\t错误原因：请检查数据库类型是否正确，例如：SqlServer、Access、MySql");
            }
        }
        #endregion constructor

        #region public static function
        public static IDataProvider GetInstance()
        {
            if (_Instance == null)
            {
                lock (lockHelper)
                {
                    if (_Instance == null)
                    {
                        GetProvider((int)DbEnumType.SqlServer);
                    }
                }
            }
            return _Instance;
        }

        public static void ResetProvider()
        {
            if (_Instance != null) _Instance = null;
        }
        #endregion public static function
    }
}
