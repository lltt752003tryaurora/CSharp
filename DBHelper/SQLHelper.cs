using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Concurrent;

namespace DBHelper
{
    /// <summary>
    /// Implement generic data access via ado ProviderFactory
    /// </summary>
    public static class SQLHelper
    {
        private static string _dbProviderName;
        private static string _dbConnectionString;

        #region Constructor
        static SQLHelper()
        {
        }

        public static void Init(object providerName, object connectionString)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Public static methods

        /// <summary>
        /// Initialize provider and connection string
        /// </summary>
        /// <param name="strProviderName">Provider name</param>
        /// <param name="strConnectionString">Connection string</param>
        public static void Init(string strProviderName, string strConnectionString)
        {
            _dbProviderName = strProviderName;
            _dbConnectionString = strConnectionString;
        }
        /// <summary>
        /// Create and return an DbCommand object
        /// </summary>
        /// <param name="cmdText">Command text</param>
        /// <param name="cmdType">Command type</param>
        /// <param name="strConstr">Database connection string</param>
        /// <param name="strProvider">Provider name</param>
        /// <returns>A new instance of DbCommand</returns>        
        public static DbCommand CreateCommand(string cmdText, CommandType cmdType, string strProvider, string strConstr)
        {
            if (string.IsNullOrEmpty(strProvider)) throw new Exception("Provider name is not set");
            if (string.IsNullOrEmpty(strConstr)) throw new Exception("Connection string is not set");

            // Create a new data provider factory
            DbProviderFactory factory = DbProviderFactories.GetFactory(strProvider);

            // Obtain a database specific connection object
            DbConnection conn = factory.CreateConnection();

            // Set connection string
            conn.ConnectionString = strConstr;

            // Create a databse specific command object
            DbCommand cmd = conn.CreateCommand();

            // Set the command type to stored procedure
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.CommandTimeout = 600;

            // Return the initialize command object
            return cmd;
        }

        /// <summary>
        /// Create and return an DbCommand object
        /// </summary>
        /// <param name="cmdText">Store procedure name</param>
        /// <returns>A new instance of DbCommand</returns>
        /// <param name="strConstr">Database connection string</param>
        /// <param name="strProvider">Provider name</param>
        public static DbCommand CreateCommand(string cmdText, string strProvider, string strConstr)
        {
            return CreateCommand(cmdText, CommandType.StoredProcedure, strProvider, strConstr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public static DbCommand CreateCommand(string cmdText)
        {
            return CreateCommand(cmdText, CommandType.Text, _dbProviderName, _dbConnectionString);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmdText"></param>
        /// <returns></returns>
        public static DbCommand GetStoredProcCommand(string cmdText)
        {
            return CreateCommand(cmdText, _dbProviderName, _dbConnectionString);
        }
        /// <summary>
        /// Execute a select command
        /// </summary>
        /// <param name="command">DbCommand object</param>
        /// <returns>A Datatable content select data</returns>
        public static DataTable ExecuteSelectCommand(DbCommand command)
        {
            // The DataTable to be returned
            DataTable table;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the data connection
                command.Connection.Open();

                // Execute the command and save the results in a DataTable
                DbDataReader reader = command.ExecuteReader();
                table = new DataTable();
                table.Load(reader);

                // Close the reader
                reader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Close the connection
                command.Connection.Dispose();
            }

            return table;
        }
        /// <summary>
        /// Execute a select command
        /// </summary>
        /// <param name="command">DbCommand object</param>
        /// <returns>A Datatable content select data</returns>
        public static DataTable ExecuteSelectCommand_doet(DbCommand command)
        {
            // The DataTable to be returned
            DataTable DT;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the data connection
                command.Connection.Open();

                // Execute the command and save the results in a DataTable
                using (DbDataReader Rdr = command.ExecuteReader())
                {
                    //Create datatable to hold schema and data seperately
                    //Get schema of our actual table
                    DataTable DTSchema = Rdr.GetSchemaTable();
                    DT = new DataTable();
                    if (DTSchema != null)
                        if (DTSchema.Rows.Count > 0)
                            for (int i = 0; i < DTSchema.Rows.Count; i++)
                            {
                                //Create new column for each row in schema table
                                //Set properties that are causing errors and add it to our datatable
                                //Rows in schema table are filled with information of columns in our actual table
                                DataColumn Col = new DataColumn(DTSchema.Rows[i]["ColumnName"].ToString(), (Type)DTSchema.Rows[i]["DataType"]);
                                Col.AllowDBNull = true;
                                Col.Unique = false;
                                Col.AutoIncrement = false;
                                DT.Columns.Add(Col);
                            }

                    while (Rdr.Read())
                    {
                        //Read data and fill it to our datatable
                        DataRow Row = DT.NewRow();
                        for (int i = 0; i < DT.Columns.Count; i++)
                        {
                            Row[i] = Rdr[i];
                        }
                        DT.Rows.Add(Row);
                    }

                    // Close the reader
                    Rdr.Close();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Close the connection
                command.Connection.Dispose();
            }

            //return table;
            return DT;
        }
        public static Dictionary<string, object> ExecuteSelectCommandDic(DbCommand command)
        {
            // The DataTable to be returned
            Dictionary<string, object> table;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the data connection
                command.Connection.Open();

                // Execute the command and save the results in a DataTable
                table = new Dictionary<string, object>();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            table.Add(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                        }
                    }
                }

                // Close the reader
                //reader.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Close the connection
                command.Connection.Dispose();
            }

            return table;
        }

        /// <summary>
        /// Execute reader a command
        /// </summary>
        /// <param name="command">Command</param>
        /// <returns>DataReader Object</returns>
        public static IDataReader ExecuteReader(DbCommand command)
        {
            // The value to be returned
            DbDataReader obj;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the connection of the command
                command.Connection.Open();

                // Execute the command                 
                obj = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // return the result
            return obj;
        }
        /// <summary>
        /// Execute reader a command
        /// </summary>
        /// <param name="command">Command</param>
        /// <returns>DataReader Object</returns>
        public static IDataReader ExecuteReaderUsingTransaction(DbCommand command)
        {
            // The value to be returned
            DbDataReader obj;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Execute the command                 
                obj = command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // return the result
            return obj;
        }

        /// <summary>
        /// Excute a non-query command
        /// </summary>
        /// <param name="command">DbCommand object</param>
        /// <returns>Number of rows effected by command</returns>
        public static int ExecuteNonQuery(DbCommand command)
        {
            // The number of affected rows
            int affectedRows = -1;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the connection of the command
                command.Connection.Open();

                // Execute the command and get the number of affected rows
                affectedRows = command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                //Logs.WriteInfo2nd("Thread 2 SqlException ex.Number = " + ex.Number
                //  + " ex.ErrorCode = " + ex.ErrorCode);
                throw ex;
            }
            catch (Exception ex)
            {
                //Logs.WriteInfo2nd("Thread 2 Exception ex errorcode = " + ex.Message);
                throw ex;
            }
            finally
            {
                // Close the connection
                command.Connection.Dispose();
            }
            // return the number of affected rows
            return affectedRows;
        }
        /// <summary>
        /// Excute a non-query command
        /// </summary>
        /// <param name="command">DbCommand object</param>
        /// <returns>Number of rows effected by command</returns>
        public static int ExecuteNonQueryUsingTransaction(DbCommand command)
        {
            // The number of affected rows
            int affectedRows = -1;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Execute the command and get the number of affected rows
                affectedRows = command.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            // return the number of affected rows
            return affectedRows;
        }

        /// <summary>
        /// Execute a scalar command
        /// </summary>
        /// <param name="command">DbCommand object</param>
        /// <returns>Scalar value (object type)</returns>
        public static object ExecuteScalar(DbCommand command)
        {
            // The value to be returned
            object obj;

            // Execute the command making sure the connection gets closed in the end
            try
            {
                // Open the connection of the command
                command.Connection.Open();

                // Execute the command                 
                obj = command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                command.Connection.Dispose();
            }
            // return the result
            return obj;
        }

        public static void AddInParameter(DbCommand cmd, string paramName, DbType dbType, object paramValue)
        {
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.DbType = dbType;
            param.Value = paramValue;
            cmd.Parameters.Add(param);
        }

        public static void AddOutParameter(DbCommand cmd, string paramName, DbType dbType)
        {
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.DbType = dbType;
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);
        }

        public static void AddOutParameter(DbCommand cmd, string paramName, DbType dbType, int size)
        {
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.DbType = dbType;
            param.Size = size;
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);
        }

        public static void AddReturnParameter(DbCommand cmd, string paramName, DbType dbType)
        {
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = paramName;
            param.DbType = dbType;
            param.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(param);
        }
        public static object Insert(string tableName, string pKeyName, Dictionary<string, object> data, bool addKey)
        {
            object id = null;
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand("");
                StringBuilder name = new StringBuilder();
                StringBuilder val = new StringBuilder();

                foreach (string key in data.Keys)
                {
                    if (addKey == false)
                    {
                        if (key != pKeyName)
                        {
                            name.Append(key);
                            name.Append(",");

                            val.Append("@" + key);
                            val.Append(",");

                            dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                        }
                    }
                    else
                    {
                        name.Append(key);
                        name.Append(",");

                        val.Append("@" + key);
                        val.Append(",");

                        dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                    }
                }
                string sql = "Insert into " + tableName + "(" + name.ToString().TrimEnd(',') + ") values(" + val.ToString().TrimEnd(',') + ");SELECT SCOPE_IDENTITY() as " + pKeyName + ";";

                dbCommand.CommandText = sql;

                using (IDataReader reader = SQLHelper.ExecuteReader(dbCommand))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(pKeyName))) id = reader[pKeyName];
                    }
                    reader.Close();
                }
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }

        }
        public static object InsertUsingTransaction(string tableName, string pKeyName, Dictionary<string, object> data, bool addKey, DbCommand dbCommand)
        {
            object id = null;
            try
            {
                dbCommand.Parameters.Clear();
                dbCommand.CommandType = CommandType.Text;
                StringBuilder name = new StringBuilder();
                StringBuilder val = new StringBuilder();

                foreach (string key in data.Keys)
                {
                    if (addKey == false)
                    {
                        if (key != pKeyName)
                        {
                            name.Append(key);
                            name.Append(",");

                            val.Append("@" + key);
                            val.Append(",");

                            dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                        }
                    }
                    else
                    {
                        name.Append(key);
                        name.Append(",");

                        val.Append("@" + key);
                        val.Append(",");

                        dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                    }
                }
                string sql = "Insert into " + tableName + "(" + name.ToString().TrimEnd(',') + ") values(" + val.ToString().TrimEnd(',') + ");SELECT SCOPE_IDENTITY() as " + pKeyName + ";";

                dbCommand.CommandText = sql;

                using (IDataReader reader = SQLHelper.ExecuteReader(dbCommand))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal(pKeyName))) id = reader[pKeyName];
                    }
                    reader.Close();
                }
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int Update(string tableName, string pKeyName, Dictionary<string, object> data)
        {
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand("");

                StringBuilder name = new StringBuilder();
                foreach (string key in data.Keys)
                {
                    if (key != pKeyName)
                    {
                        name.Append(key + " = @" + key);
                        name.Append(",");
                        dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                    }
                }
                string sql = "update " + tableName + " set " + name.ToString().TrimEnd(',') + " where " + pKeyName + " = @" + pKeyName + ";";
                dbCommand.Parameters.Add(new SqlParameter("@" + pKeyName, data[pKeyName]));

                dbCommand.CommandText = sql;

                return SQLHelper.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int UpdateUsingTransaction(string tableName, string pKeyName, Dictionary<string, object> data, DbCommand dbCommand)
        {
            try
            {
                dbCommand.Parameters.Clear();
                dbCommand.CommandType = CommandType.Text;

                StringBuilder name = new StringBuilder();
                foreach (string key in data.Keys)
                {
                    if (key != pKeyName)
                    {
                        name.Append(key + " = @" + key);
                        name.Append(",");
                        dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                    }
                }
                string sql = "update " + tableName + " set " + name.ToString().TrimEnd(',') + " where " + pKeyName + " = @" + pKeyName + ";";
                dbCommand.Parameters.Add(new SqlParameter("@" + pKeyName, data[pKeyName]));

                dbCommand.CommandText = sql;

                return SQLHelper.ExecuteNonQueryUsingTransaction(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int Update(string tableName, List<string> pKeyName, Dictionary<string, object> data)
        {
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand("");

                StringBuilder name = new StringBuilder();
                foreach (string key in data.Keys)
                {
                    if (!pKeyName.Contains(key))
                    {
                        name.Append(key + " = @" + key);
                        name.Append(",");
                        dbCommand.Parameters.Add(new SqlParameter("@" + key, data[key]));
                    }
                }

                string where = "";
                foreach (string s in pKeyName)
                {
                    where += " " + s + " = @" + s + " AND";
                    dbCommand.Parameters.Add(new SqlParameter("@" + s, data[s]));
                }

                string sql = "update " + tableName + " set " + name.ToString().TrimEnd(',') + " where " + where.TrimEnd("AND".ToCharArray()) + ";";

                dbCommand.CommandText = sql;

                return SQLHelper.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int Delete(string tableName, string pKeyName, string ids)
        {
            string sql = "delete from  " + tableName + " where " + pKeyName + " in (" + ids + ");";
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand(sql);
                return SQLHelper.ExecuteNonQuery(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static Dictionary<string, object> FindOne(string tableName, string field, object value)
        {
            Dictionary<string, KeyValuePair<string, object>> searchDic = new Dictionary<string, KeyValuePair<string, object>>();
            searchDic.Add(field, new KeyValuePair<string, object>("=", value));
            List<object> items = SQLHelper.Find(tableName, searchDic, "OR");
            if (items != null && items.Count > 0)
            {
                return (Dictionary<string, object>)items[0];
            }
            return null;
        }
        public static List<object> Find(string tableName, string field, object value)
        {
            Dictionary<string, KeyValuePair<string, object>> searchDic = new Dictionary<string, KeyValuePair<string, object>>();
            searchDic.Add(field, new KeyValuePair<string, object>("=", value));
            return SQLHelper.Find(tableName, searchDic, "OR");
        }
        public static List<object> Find(string tableName, Dictionary<string, KeyValuePair<string, object>> findCondition, string condition)
        {
            if (string.IsNullOrEmpty(condition)) condition = "OR";
            condition = " " + condition + " ";
            List<object> rs = new List<object>();
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand("");
                StringBuilder whereCondition = new StringBuilder();
                foreach (string key in findCondition.Keys)
                {
                    KeyValuePair<string, object> keyPair = findCondition[key];
                    whereCondition.Append(key);
                    whereCondition.Append(" ");
                    whereCondition.Append(keyPair.Key);
                    whereCondition.Append(" ");
                    if (keyPair.Key.ToLower().Trim() == "in")
                    {
                        whereCondition.Append(keyPair.Value);
                        whereCondition.Append(condition);
                    }
                    else
                    {
                        whereCondition.Append("@" + key);
                        whereCondition.Append(condition);
                        dbCommand.Parameters.Add(new SqlParameter("@" + key, keyPair.Value));
                    }
                }

                string sql = "select * from  " + tableName + " where " + whereCondition.ToString().TrimEnd(condition.ToCharArray()) + ";";
                dbCommand.CommandText = sql;

                using (IDataReader reader = SQLHelper.ExecuteReader(dbCommand))
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            item.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        rs.Add(item);
                    }
                    reader.Close();
                }

                return rs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }
        }
        public static List<object> Fetch(string tableName, Dictionary<string, KeyValuePair<string, object>> findCondition, string conditionOR_AND, string lc, string orderBy, string orderDirection, int page, int pageSize, out int totalRecords)
        {
            totalRecords = 0;
            if (string.IsNullOrEmpty(conditionOR_AND)) conditionOR_AND = "OR";
            conditionOR_AND = " " + conditionOR_AND + " ";
            List<object> rs = new List<object>();
            DbCommand dbCommand = null;
            try
            {
                dbCommand = SQLHelper.CreateCommand("");

                StringBuilder whereCondition = new StringBuilder();
                foreach (string key in findCondition.Keys)
                {
                    KeyValuePair<string, object> keyPair = findCondition[key];
                    whereCondition.Append(key);
                    whereCondition.Append(" ");
                    whereCondition.Append(keyPair.Key);
                    whereCondition.Append(" ");

                    if (keyPair.Key.ToLower().Trim() == "in")
                    {
                        whereCondition.Append(keyPair.Value);
                        whereCondition.Append(conditionOR_AND);
                    }
                    else
                    {
                        whereCondition.Append("@" + key);
                        whereCondition.Append(conditionOR_AND);
                        dbCommand.Parameters.Add(new SqlParameter("@" + key, keyPair.Value));
                    }


                    //whereCondition.Append("@" + key);
                    //whereCondition.Append(conditionOR_AND);

                    //dbCommand.Parameters.Add(new SqlParameter(key, keyPair.Value));
                }

                string sql = "";// "select * from  " + tableName + " where " + whereCondition.ToString().TrimEnd(conditionOR_AND.ToCharArray()) + ";";
                string where = whereCondition.ToString().TrimEnd(conditionOR_AND.ToCharArray());
                if (string.IsNullOrEmpty(lc))
                {
                    if (string.IsNullOrEmpty(where)) where = "1=1";
                    sql = string.Format(@"BEGIN
	                            WITH {0}List AS (
		                            SELECT
			                            ROW_NUMBER() OVER (ORDER BY {1} {2}
			                            ) AS ROWNUMBER,
			                            *
			                            FROM {3}
                                        WHERE {4}
		                            )
		                            SELECT 
			                            *
			                            , '' as lc
			                            FROM {5}List
			                            WHERE ({6} = 0) OR ({7} > 0 AND ({8} - 1)*{9} < ROWNUMBER AND ROWNUMBER <= {10}*{11})
                            END;", tableName, orderBy, orderDirection, tableName, where, tableName, pageSize, pageSize, page, pageSize, page, pageSize);
                    sql += string.Format(@"SELECT COUNT(*) as TotalRecords
		                    FROM {0}
		                    WHERE
		                    {1};", tableName, where);
                }
                else
                {
                    if (string.IsNullOrEmpty(where)) where = "1=1";
                    tableName = tableName + "_language";

                    sql = string.Format(@"BEGIN
	                            WITH {0}List AS (
		                            SELECT
			                            ROW_NUMBER() OVER (ORDER BY {1} {2}
			                            ) AS ROWNUMBER,
			                            *
			                            FROM {3}
                                        WHERE {4}
		                            )
		                            SELECT 
			                            *
			                            ,lc
			                            FROM {5}List
			                            WHERE ({6} = 0) OR ({7} > 0 AND ({8} - 1)*{9} < ROWNUMBER AND ROWNUMBER <= {10}*{11})
                            END;", tableName, orderBy, orderDirection, tableName, where + " AND lc = '" + lc + "'", tableName, pageSize, pageSize, page, pageSize, page, pageSize);
                    sql += string.Format(@"SELECT COUNT(*) as TotalRecords
		                    FROM {0}
		                    WHERE
		                    {1};", tableName, where + " AND lc = '" + lc + "'");
                }
                dbCommand.CommandText = sql;


                using (IDataReader reader = SQLHelper.ExecuteReader(dbCommand))
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            item.Add(reader.GetName(i), reader.GetValue(i));
                        }
                        rs.Add(item);
                    }
                    reader.NextResult();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("TotalRecords"))) totalRecords = (int)reader["TotalRecords"];
                    }
                    reader.Close();
                }
                return rs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }
        }

        public static List<string> GetTables()
        {
            DbCommand dbCommand = null;
            try
            {
                List<string> lstTables = new List<string>();
                dbCommand = SQLHelper.CreateCommand("SELECT name FROM sys.Tables");
                using (IDataReader reader = SQLHelper.ExecuteReader(dbCommand))
                {
                    while (reader.Read())
                    {
                        lstTables.Add(reader["name"].ToString());
                    }
                    reader.Close();
                }
                return lstTables;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dbCommand != null) dbCommand.Connection.Dispose();
            }
        }
        public static bool isDefault(object obj)
        {
            bool isDefault = false;
            if (obj.GetType() == typeof(string))
            {
                isDefault = (obj == null || string.IsNullOrEmpty(obj.ToString()));
            }
            if (obj.GetType() == typeof(int) || obj.GetType() == typeof(Int16) || obj.GetType() == typeof(Int32) || obj.GetType() == typeof(Int64) || obj.GetType() == typeof(long) || obj.GetType() == typeof(decimal) || obj.GetType() == typeof(double) || obj.GetType() == typeof(float))
            {
                isDefault = (obj == null || obj.ToString() == "0");
            }
            if (obj.GetType() == typeof(DateTime))
            {
                isDefault = (obj == null || (DateTime)obj == new DateTime(1900, 1, 1, 0, 0, 0, 0));
            }
            return isDefault;
        }
        #endregion
    }
}