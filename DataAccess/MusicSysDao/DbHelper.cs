using IMusicSysDao;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSysDao
{
    /// <summary>
    /// 数据访问助手类 (MySql -- SqlServer -- Oracle)
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        /// 无效连接
        /// </summary>
        private const int ConnError = -100;
        /// <summary>
        /// 执行SQL语句失败
        /// </summary>
        private const int ExecuteError = -200;
        /// <summary>
        /// 无效事务
        /// </summary>
        private const int TransError = -300;

        #region 私有变量
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        protected static string m_connectionstring = null;

        /// <summary>
        /// DbProviderFactory实例
        /// </summary>
        private static DbProviderFactory m_factory = null;

        /// <summary>
        /// 数据接口
        /// </summary>
        private static IDbProvider m_provider = null;

        /// <summary>
        /// 查询次数统计
        /// </summary>
        private static int m_querycount = 0;
        /// <summary>
        /// Parameters缓存哈希表
        /// </summary>
        private static Hashtable m_paramcache = Hashtable.Synchronized(new Hashtable());
        private static object lockHelper = new object();
        #endregion

        #region 属性
        /// <summary>
        /// 查询次数统计
        /// </summary>
        public static int QueryCount
        {
            get { return m_querycount; }
            set { m_querycount = value; }
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (m_connectionstring == null)
                {
                    m_connectionstring = DbHelperConfig.m_connectionstring;
                }
                return m_connectionstring;
            }
            set
            {
                m_connectionstring = value;
            }
        }

        /// <summary>
        /// IDbProvider接口
        /// </summary>
        public static IDbProvider Provider
        {
            get
            {
                if (m_provider == null)
                {
                    lock (lockHelper)
                    {
                        if (m_provider == null)
                        {
                            try
                            {
                                m_provider = (IDbProvider)Activator.CreateInstance(Type.GetType("MusicSysDao." + DbHelperConfig.m_dbtype + "Provider", false, true));
                            }
                            catch
                            {
                                //LogBLL.error(PageInfo + "方法名：Provider\t\t错误原因：请检查数据库类型是否正确，例如：SqlServer、Access、MySql");
                            }
                        }
                    }
                }
                return m_provider;
            }
        }

        /// <summary>
        /// DbFactory实例
        /// </summary>
        public static DbProviderFactory Factory
        {
            get
            {
                if (m_factory == null)
                {
                    m_factory = Provider.Instance();
                }
                return m_factory;
            }
        }

        /// <summary>
        /// 刷新数据库提供者
        /// </summary>
        public static void ResetDbProvider()
        {
            try
            {
                m_connectionstring = null;
                m_factory = null;
                m_provider = null;
                MusicSysDao.Provider.ResetProvider();
            }
            catch (Exception ex)
            { //LogBLL.error(PageInfo + "方法名：ResetDbProvider\t\t", ex); 
            }
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 将DbParameter参数数组(参数值)分配给DbCommand命令.
        /// 这个方法将给任何一个参数分配DBNull.Value;
        /// 该操作将阻止默认值的使用.
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="commandParameters">DbParameters数组</param>
        private static void AttachParameters(DbCommand command, DbParameter[] commandParameters)
        {
            if (command == null) return;
            if (commandParameters == null) return;
            foreach (DbParameter p in commandParameters)
            {
                if (p != null)
                {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }
                    command.Parameters.Add(p);
                }
            }
        }

        /// <summary>
        /// 将一个对象数组分配给DbParameter参数数组.
        /// </summary>
        /// <param name="commandParameters">要分配值的DbParameter参数数组</param>
        /// <param name="parameterValues">将要分配给存储过程参数的对象数组</param>
        private static void AssignParameterValues(DbParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }
            // 确保对象数组个数与参数个数匹配,如果不匹配,抛出一个异常.
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("参数值个数与参数不匹配.");
            }
            // 给参数赋值
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// 预处理用户提供的命令,数据库连接/事务/命令类型/参数
        /// </summary>
        /// <param name="command">要处理的DbCommand</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">一个有效的事务或者是null值</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名或都SQL命令文本</param>
        /// <param name="commandParameters">和命令相关联的DbParameter参数数组,如果没有参数为'null'</param>
        private static void PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, CommandType commandType
            , string commandText, DbParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command is null");
            if (String.IsNullOrEmpty(commandText)) throw new ArgumentNullException("commandText is null");
            //打开连接
            if (connection.State != ConnectionState.Open) connection.Open();
            // 给命令分配一个数据库连接.
            command.Connection = connection;
            //设置命令超时时间
            command.CommandTimeout = DbHelperConfig.m_commandtimeout;
            // 设置命令文本(存储过程名或SQL语句)
            command.CommandText = commandText;
            // 分配事务
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }
            // 设置命令类型.
            command.CommandType = commandType;
            // 分配命令参数
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
        }

        /// <summary>
        /// 探索运行时的存储过程,返回DbParameter参数数组.
        /// 初始化参数值为 DBNull.Value.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        private static DbParameter[] DiscoverSpParameterSet(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");
            DbCommand cmd = connection.CreateCommand();
            cmd.CommandTimeout = DbHelperConfig.m_commandtimeout;
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            // 检索cmd指定的存储过程的参数信息,并填充到cmd的Parameters参数集中.
            Provider.DeriveParameters(cmd);
            connection.Close();
            // 如果不包含返回值参数,将参数集中的每一个参数删除.
            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }
            // 创建参数数组
            DbParameter[] discoveredParameters = new DbParameter[cmd.Parameters.Count];
            // 将cmd的Parameters参数集复制到discoveredParameters数组.
            cmd.Parameters.CopyTo(discoveredParameters, 0);
            // 初始化参数值为 DBNull.Value.
            foreach (DbParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// DbParameter参数数组的深层拷贝.
        /// </summary>
        /// <param name="originalParameters">原始参数数组</param>
        /// <returns>返回一个同样的参数数组</returns>
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] clonedParameters = new DbParameter[originalParameters.Length];
            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }
            return clonedParameters;
        }

        #endregion 私有方法结束

        #region 测试连接
        /// <summary>
        /// 判断连接字符串是否有效，True有效
        /// </summary>
        /// <param name="TestConnectionString">要测试的连接字符串</param>
        /// <returns></returns>
        public static bool OpenConn(string TestConnectionString)
        {
            try
            {
                if (TestConnectionString == null || TestConnectionString.Length == 0) return false;
                if (Factory == null) return false;
                using (DbConnection connection = Factory.CreateConnection())
                {
                    connection.ConnectionString = TestConnectionString;
                    connection.Open();
                    DbCommand cmd = Factory.CreateCommand();
                    cmd.CommandTimeout = DbHelperConfig.m_commandtimeout;
                    cmd.CommandText = "select 1";
                    cmd.Connection = connection;
                    cmd.ExecuteScalar();
                    connection.Close();
                    return true;
                }
            }
            catch (Exception ex) { }
            //{ LogBLL.error(PageInfo + "方法名：OpenConn\t\t", ex); }
            return false;
        }

        /// <summary>
        /// 判断当前连接是否有效，True有效
        /// </summary>
        /// <returns></returns>
        public static bool OpenConn()
        {
            return OpenConn(DbHelperConfig.m_connectionstring);
        }
        #endregion

        #region 获取活动连接对象
        public static DbConnection GetConnection()
        {
            try
            {
                DbConnection connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：GetConnection\t\t", ex);
                return null;
            }
        }

        public static DbTransaction GetTransaction()
        {
            try
            {
                return GetConnection().BeginTransaction();
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：GetTransaction\t\t", ex);
                return null;
            }
        }

        public static DbCommand GetCommand()
        {
            try
            {
                DbCommand cmd = Factory.CreateCommand();
                return cmd;
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：GetCommand\t\t", ex);
                return null;
            }
        }

        public static DbDataAdapter GetDataAdapter()
        {
            try
            {
                DbDataAdapter da = Factory.CreateDataAdapter();
                return da;
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：GetDataAdapter\t\t", ex);
                return null;
            }
        }
        #endregion

        #region 使用SqlBulkCopy复制入数据库
        public static string ExecuteBulkCopy(string tablename, DataTable dt)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    SqlConnection _conn = (SqlConnection)connection;
                    _conn.ConnectionString = ConnectionString;
                    _conn.Open();
                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(_conn))
                    {
                        bulkcopy.DestinationTableName = tablename;
                        bulkcopy.WriteToServer(dt);
                        bulkcopy.Close();
                    }
                    _conn.Close();
                    _conn.Dispose();
                    return "success";
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteBulkCopy\t\t", ex);
                    return ex.Message;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }
        #endregion

        #region ExecuteNonQuery方法
        #region ExecuteNonQuery
        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) return ConnError;
            if (String.IsNullOrEmpty(spName)) return ExecuteError;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    // 从缓存中加载存储过程参数
                    DbParameter[] commandParameters = GetSpParameterSet(connection, spName);
                    // 给存储过程分配参数值
                    AssignParameterValues(commandParameters, parameterValues);
                    return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand。
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (connection == null) return ConnError;
            // 创建DbCommand命令,并进行预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters);
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }
        #endregion

        #region ExecuteNonQuery，返回ID
        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand，并返回刚插入的自增ID。
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out long id, string commandText)
        {
            return ExecuteNonQuery(out id, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand，并返回刚插入的自增ID。
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="commandType">命令类型 (存储过程，命令文本, 其它。)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(out long id, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand，并返回刚插入的自增ID。
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out long id, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            id = -1;
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    return ExecuteNonQuery(out id, connection, commandType, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand，并返回刚插入的自增ID。
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out long id, DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串，类型的DbCommand，并返回刚插入的自增ID。
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out long id, DbConnection connection, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            id = -1;
            if (connection == null) return ConnError;
            // 创建DbCommand命令,并进行预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters);
                    int rval = cmd.ExecuteNonQuery();
                    // 取ID.
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Provider.GetLastIdSql();
                    id = long.Parse(cmd.ExecuteScalar().ToString());
                    return rval;
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }
        #endregion

        #region ExecuteNonQuery，带事务
        /// <summary>
        /// 执行带事务的DbCommand.
        /// </summary>
        /// <param name="transaction">一个有效的数据库事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数/returns>
        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行带事务的DbCommand(指定参数值).
        /// </summary>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) return TransError;
            if (transaction.Connection == null) return ConnError;
            if (String.IsNullOrEmpty(spName)) return ExecuteError;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中
                    DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);
                    // 给存储过程参数赋值
                    AssignParameterValues(commandParameters, parameterValues);
                    // 调用重载方法
                    return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行带事务的DbCommand(指定参数).
        /// </summary>
        /// <param name="transaction">一个有效的数据库事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (transaction == null) return TransError;
            if (transaction.Connection == null) return ConnError;
            if (String.IsNullOrEmpty(commandText)) return ExecuteError;
            // 预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
        }

        /// <summary>
        /// 执行带事务的DbCommand，并返回刚插入的自增ID
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="transaction">一个有效的数据库事务</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数/returns>
        public static int ExecuteNonQuery(out long id, DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行带事务的DbCommand(指定参数)，并返回刚插入的自增ID
        /// </summary>
        /// <param name="id">返回刚插入的自增ID</param>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out long id, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            id = -1;
            if (transaction == null) return TransError;
            if (transaction.Connection == null) return ConnError;
            if (String.IsNullOrEmpty(commandText)) return ExecuteError;
            // 预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    int rval = cmd.ExecuteNonQuery();
                    // 取ID
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = Provider.GetLastIdSql();
                    id = long.Parse(cmd.ExecuteScalar().ToString());
                    return rval;
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteNonQuery\t\t", ex);
                    return ExecuteError;
                }
            }
        }
        #endregion
        #endregion ExecuteNonQuery方法结束

        #region ExecuteDataset方法
        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(string commandText)
        {
            return ExecuteDataset(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(CommandType commandType, string commandText)
        {
            return ExecuteDataset(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    return ExecuteDataset(connection, commandType, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,返回DataSet.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定存储过程参数,返回DataSet.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (connection == null) return null;
            if (String.IsNullOrEmpty(commandText)) return null;
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                try
                {
                    using (DbCommand cmd = Factory.CreateCommand())
                    {
                        PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters);
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        return ds;
                    }
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,直接提供参数值,返回DataSet.
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(string spName, params object[] parameterValues)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    return ExecuteDataset(connection, spName, parameterValues);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数值,返回DataSet.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                try
                {
                    // 从缓存中加载存储过程参数
                    DbParameter[] commandParameters = GetSpParameterSet(connection, spName);
                    // 给存储过程参数分配值
                    AssignParameterValues(commandParameters, parameterValues);
                    return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
            }
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定事务的命令,返回DataSet.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定事务的命令,指定参数,返回DataSet.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            if (String.IsNullOrEmpty(commandText)) return null;
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                try
                {
                    using (DbCommand cmd = Factory.CreateCommand())
                    {
                        PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                        da.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        return ds;
                    }
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定事务的命令,指定参数值,返回DataSet.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                try
                {
                    // 从缓存中加载存储过程参数
                    DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);
                    // 给存储过程参数分配值
                    AssignParameterValues(commandParameters, parameterValues);
                    return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteDataset\t\t", ex);
                    return null;
                }
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion ExecuteDataset数据集命令结束

        #region ExecuteReader 数据阅读器
        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器.
        /// </summary>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(CommandType.Text, commandText);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return ExecuteReader(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器,指定参数.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            try
            {
                DbConnection connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteReader(connection, commandType, commandText, commandParameters);
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex);
                return null;
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的数据阅读器.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器,指定参数.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (connection == null) return null;
            if (String.IsNullOrEmpty(commandText)) return null;
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters);
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器,指定参数值.
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(string spName, params object[] parameterValues)
        {
            try
            {
                DbConnection connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();
                return ExecuteReader(connection, spName, parameterValues);
            }
            catch (Exception ex)
            {
                //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex);
                return null;
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的数据阅读器,指定参数值.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    DbParameter[] commandParameters = GetSpParameterSet(connection, spName);
                    AssignParameterValues(commandParameters, parameterValues);
                    return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex);
                    return null;
                }
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库事务的数据阅读器,指定参数值.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库事务的数据阅读器,指定参数值.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);
                    AssignParameterValues(commandParameters, parameterValues);
                    return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex); 
                }
                return null;
            }
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的数据阅读器.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">DbParameters参数数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        private static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            if (String.IsNullOrEmpty(commandText)) return null;
            // 创建命令
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    return cmd.ExecuteReader();
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteReader\t\t", ex);
                    return null;
                }
            }
        }
        #endregion ExecuteReader数据阅读器

        #region ExecuteScalar 返回结果集中的第一行第一列
        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText)
        {
            // 执行参数为空的方法
            return ExecuteScalar(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            // 创建并打开数据库连接对象,操作完成释放对象.
            using (DbConnection connection = Factory.CreateConnection())
            {
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();
                    return ExecuteScalar(connection, commandType, commandText, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(string spName, params object[] parameterValues)
        {
            if (String.IsNullOrEmpty(spName)) return null;
            // 如果有参数值
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                    DbParameter[] commandParameters = GetSpParameterSet(spName);
                    // 给存储过程参数赋值
                    AssignParameterValues(commandParameters, parameterValues);
                    // 调用重载方法
                    return ExecuteScalar(CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
            }
            else
            {
                return ExecuteScalar(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (connection == null) return null;
            // 创建DbCommand命令,并进行预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters);
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            // 如果有参数值
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                    DbParameter[] commandParameters = GetSpParameterSet(connection, spName);
                    // 给存储过程参数赋值
                    AssignParameterValues(commandParameters, parameterValues);
                    // 调用重载方法
                    return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
            }
            else
            {
                // 没有参数值
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库事务的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库事务的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的DbParameter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            // 创建DbCommand命令,并进行预处理
            using (DbCommand cmd = Factory.CreateCommand())
            {
                try
                {
                    PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters);
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行指定数据库事务的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="transaction">一个有效的事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) return null;
            if (transaction.Connection == null) return null;
            if (String.IsNullOrEmpty(spName)) return null;
            // 如果有参数值
            if (parameterValues != null && parameterValues.Length > 0)
            {
                try
                {
                    DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);
                    // 给存储过程参数赋值
                    AssignParameterValues(commandParameters, parameterValues);
                    // 调用重载方法
                    return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
                }
                catch (Exception ex)
                {
                    //LogBLL.error(PageInfo + "方法名：ExecuteScalar\t\t", ex);
                    return null;
                }
            }
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion ExecuteScalar

        #region ExecuteScalarToStr，返回结果集中的第一行第一列，并将结果以字符串类型输出
        public static string ExecuteScalarToStr(string commandText)
        {
            return ExecuteScalarToStr(CommandType.Text, commandText);
        }

        public static string ExecuteScalarToStr(CommandType commandType, string commandText)
        {
            return ExecuteScalarToStr(commandType, commandText, (DbParameter[])null);
        }

        public static string ExecuteScalarToStr(CommandType commandType, string commandText, DbParameter[] commandParameters)
        {
            object ec = ExecuteScalar(commandType, commandText, commandParameters);
            if (ec == null) return String.Empty;
            return ec.ToString();
        }

        public static string ExecuteScalarToStr(DbTransaction transaction, string commandText)
        {
            return ExecuteScalarToStr(transaction, CommandType.Text, commandText);
        }

        public static string ExecuteScalarToStr(DbTransaction transaction, CommandType commandType, string commandText)
        {
            object ec = ExecuteScalar(transaction, commandType, commandText);
            if (ec == null) return String.Empty;
            return ec.ToString();
        }
        #endregion

        #region ExecuteCommandWithSplitter方法
        /// <summary>
        /// 运行含有GO命令的多条SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令字符串</param>
        /// <param name="splitter">分割字符串</param>
        public static void ExecuteCommandWithSplitter(string commandText, string splitter)
        {
            int startPos = 0;
            do
            {
                int lastPos = commandText.IndexOf(splitter, startPos);
                int len = (lastPos > startPos ? lastPos : commandText.Length) - startPos;
                string query = commandText.Substring(startPos, len);
                if (query.Trim().Length > 0)
                {
                    ExecuteNonQuery(CommandType.Text, query);
                }
                if (lastPos == -1)
                {
                    break;
                }
                else
                {
                    startPos = lastPos + splitter.Length;
                }
            }
            while (startPos < commandText.Length);
        }

        /// <summary>
        /// 运行含有GO命令的多条SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令字符串</param>
        public static void ExecuteCommandWithSplitter(string commandText)
        {
            ExecuteCommandWithSplitter(commandText, "\r\nGO\r\n");
        }
        #endregion ExecuteCommandWithSplitter方法结束

        #region CreateCommand 创建一条DbCommand命令
        /// <summary>
        /// 创建DbCommand命令,指定数据库连接对象,存储过程名和参数.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="sourceColumns">源表的列名称数组</param>
        /// <returns>返回DbCommand命令</returns>
        public static DbCommand CreateCommand(DbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                return null;
            }
            if (String.IsNullOrEmpty(spName))
            {
                return null;
            }
            // 创建命令
            DbCommand cmd = Factory.CreateCommand();
            cmd.CommandText = spName;
            cmd.Connection = connection;
            cmd.CommandType = CommandType.StoredProcedure;
            // 如果有参数值
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);
                // 将源表的列到映射到DataSet命令中.
                for (int index = 0; index < sourceColumns.Length; index++)
                    commandParameters[index].SourceColumn = sourceColumns[index];
                // Attach the discovered parameters to the DbCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }
        #endregion

        #region 缓存方法
        /// <summary>
        /// 追加参数数组到缓存.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">要缓存的参数数组</param>
        public static void CacheParameterSet(string commandText, DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");
            string hashKey = ConnectionString + ":" + commandText;
            m_paramcache[hashKey] = commandParameters;
        }

        /// <summary>
        /// 从缓存中获取参数数组.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>参数数组</returns>
        public static DbParameter[] GetCachedParameterSet(string commandText)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");
            string hashKey = ConnectionString + ":" + commandText;
            DbParameter[] cachedParameters = m_paramcache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }
        #endregion 缓存方法结束

        #region 检索指定的存储过程的参数集
        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符</param>
        /// <param name="spName">存储过程名</param>
        /// <returns>返回DbParameter参数数组</returns>
        public static DbParameter[] GetSpParameterSet(string spName)
        {
            return GetSpParameterSet(spName, false);
        }

        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符.</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        public static DbParameter[] GetSpParameterSet(string spName, bool includeReturnValueParameter)
        {
            if (ConnectionString == null || ConnectionString.Length == 0)
            {
                return null;
            }
            if (spName == null || spName.Length == 0)
            {
                return null;
            }
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象).
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接字符</param>
        /// <param name="spName">存储过程名</param>
        /// <returns>返回DbParameter参数数组</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">
        /// 是否包含返回值参数
        /// </param>
        /// <returns>返回DbParameter参数数组</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (DbConnection clonedConnection = (DbConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// [私有]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        private static DbParameter[] GetSpParameterSetInternal(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                return null;
            }
            if (String.IsNullOrEmpty(spName))
            {
                return null;
            }
            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : String.Empty);
            DbParameter[] cachedParameters;
            cachedParameters = m_paramcache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                DbParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                m_paramcache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }
            return CloneParameters(cachedParameters);
        }
        #endregion 参数集检索结束

        #region 生成参数
        public static DbParameter MakeInParam(string ParamName, DbType DbType, int Size, object Value, bool format)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value, format);
        }

        public static DbParameter MakeInParam(string ParamName, DbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value, false);
        }

        public static DbParameter MakeOutParam(string ParamName, DbType DbType, int Size)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null, false);
        }

        public static DbParameter MakeParam(string ParamName, DbType DbType, Int32 Size, ParameterDirection Direction, object Value, bool format)
        {
            SqlServerProvider sp = new SqlServerProvider();
            DbParameter param;
            param = sp.MakeParam(ParamName, DbType, Size);
            param.Direction = Direction;
            //if (!(Direction == ParameterDirection.Output && Value == null))
            //{
            //    param.Value = Value;
            //}
            if (Value != null && Value.ToString() != String.Empty)
            {
                if (!format)
                {
                    param.Value = Value;
                }
                else
                {
                    param.Value = RegEsc(Value.ToString());
                }
            }
            else
            {
                param.Value = DBNull.Value;
            }
            return param;
        }

        /// <summary>
        /// SQL语句转义
        /// </summary>
        /// <param name="str">需要转义的关键字符串</param>
        /// <returns>转义后的字符串</returns>
        public static string RegEsc(string str)
        {
            return str.Replace("%", "[%]")
                        .Replace("_", "[_]")
                        .Replace("[", "[[]")
                        .Replace("'", "''''");
        }
        #endregion 生成参数结束

        #region 生成参数
        #endregion 生成参数结束

        #region 脚本处理
        /// <summary>
        /// 新增数据时链接SQL
        /// </summary>
        /// <param name="Fields"></param>
        /// <param name="Values"></param>
        /// <param name="Field"></param>
        /// <param name="Value"></param>
        /// <param name="DefaultValue"></param>
        public static void AssignAdd(ref string Fields, ref string Values, string Field, string Value, string DefaultValue)
        {
            Fields += (String.IsNullOrEmpty(Fields) ? String.Empty : ",") + Field;
            if (!String.IsNullOrEmpty(Value))
            {
                Values += (String.IsNullOrEmpty(Values) ? String.Empty : ",") + "'" + Value + "'";
            }
            else
            {
                if (String.IsNullOrEmpty(DefaultValue)) DefaultValue = "NULL";
                Values += (String.IsNullOrEmpty(Values) ? String.Empty : ",") + DefaultValue;
            }
        }

        /// <summary>
        /// 新增数据时链接SQL
        /// </summary>
        /// <param name="Fields"></param>
        /// <param name="Values"></param>
        /// <param name="Field"></param>
        /// <param name="Value"></param>
        public static void AssignAdd(ref string Fields, ref string Values, string Field, string Value)
        {
            AssignAdd(ref Fields, ref Values, Field, Value, null);
        }

        /// <summary>
        /// 修改数据时链接SQL
        /// </summary>
        /// <param name="Assign"></param>
        /// <param name="Field"></param>
        /// <param name="Value"></param>
        public static void AssignUpdate(ref string Assign, string Field, string Value, string DefaultValue)
        {
            if (!String.IsNullOrEmpty(Value))
            {
                Assign += (String.IsNullOrEmpty(Assign) ? String.Empty : ",") + Field + "='" + Value + "'";
            }
            else
            {
                if (String.IsNullOrEmpty(DefaultValue)) DefaultValue = "NULL";
                Assign += (String.IsNullOrEmpty(Assign) ? String.Empty : ",") + Field + "=" + DefaultValue;
            }
        }

        /// <summary>
        /// 修改数据时链接SQL
        /// </summary>
        /// <param name="Assign"></param>
        /// <param name="Field"></param>
        /// <param name="Value"></param>
        public static void AssignUpdate(ref string Assign, string Field, string Value)
        {
            AssignUpdate(ref Assign, Field, Value, String.Empty);
        }

        /// <summary>
        /// 连接存储过程的参数
        /// </summary>
        /// <param name="Sql"></param>
        /// <param name="Value"></param>
        public static void AssignProcPara(ref string ParaString, string ParaValue)
        {
            if (!String.IsNullOrEmpty(ParaValue))
            {
                ParaString += (String.IsNullOrEmpty(ParaString) ? " " : " ,") + "'" + ParaValue + "'";
            }
            else
            {
                ParaString += (String.IsNullOrEmpty(ParaString) ? " " : " ,") + "NULL";
            }
        }
        #endregion

        #region 日志管理
        /// <summary>
        /// 分离参数信息
        /// </summary>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        private static string GetParameters(DbParameter[] Parameters)
        {
            if (Parameters == null) return String.Empty;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (DbParameter p in Parameters)
            {
                if (first)
                {
                    sb.Append(" ");
                    first = false;
                }
                else
                {
                    sb.Append(",");
                }
                sb.Append(p.ParameterName);
                sb.Append("='");
                sb.Append(p.Value);
                sb.Append("'");
            }
            return sb.ToString();
        }
        #endregion
    }
}
