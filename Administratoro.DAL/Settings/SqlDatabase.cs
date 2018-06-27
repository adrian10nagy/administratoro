
namespace Administratoro.DAL.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;

    public class SqlDatabase<ConnectionType, CommandType, ParameterType>
        where ConnectionType : IDbConnection, new()
        where CommandType : IDbCommand, new()
        where ParameterType : IDbDataParameter, new()
    {
        #region variabiles

        public static string ConnectionStringSet { get; set; }
        public static System.Data.CommandType CommandTypeSet { get; set; }
        public delegate void ReadRow(IDataReader reader);

        #endregion

        public SqlDatabase()
        {
            Set(Configure.ConnectionString, System.Data.CommandType.StoredProcedure);
        }

        void Set(string connectionString, System.Data.CommandType commandType)
        {
            ConnectionStringSet = connectionString;
            CommandTypeSet = commandType;
        }

        #region Execute
        public List<Object> Execute(string query, ParameterType[] parameters)
        {
            return Execute(ConnectionStringSet, CommandTypeSet, query, parameters);
        }

        public List<Object> Execute(string connectionString, System.Data.CommandType commandType, string query, ParameterType[] parameters)
        {

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    connection.ConnectionString = connectionString;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        Array.ForEach(parameters, p => command.Parameters.Add(p));
                    }

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        var Object = new Object();

                        List<Object> personList = new List<Object>();


                        while (reader.Read())
                        {
                            foreach (var column in columns)
                            {
                                var col = reader[column];
                            }
                        }
                    }
                    var results = new List<Object>();

                    connection.Close();
                    return results;
                }
            }
        }

        #endregion

        public void Execute(string query, ParameterType[] parameters, ReadRow rowMethod)
        {
            Execute(ConnectionStringSet, System.Data.CommandType.StoredProcedure, query, parameters, rowMethod);
        }

        public void Execute(System.Data.CommandType commandType, string query, ParameterType[] parameters, ReadRow rowMethod)
        {
            Execute(ConnectionStringSet, commandType, query, parameters, rowMethod);
        }

        public void Execute(string connectionString, string query, ParameterType[] parameters, ReadRow rowMethod)
        {
            Execute(connectionString, System.Data.CommandType.StoredProcedure, query, parameters, rowMethod);
        }

        public void Execute(string connectionString, System.Data.CommandType commandType, string query, ParameterType[] parameters, ReadRow rowMethod)
        {
            using (var connection = new ConnectionType())
            {
                using (var command = new CommandType())
                {
                    command.Connection = connection;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        Array.ForEach(parameters, p => command.Parameters.Add(p));
                    }

                    connection.ConnectionString = connectionString;
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (rowMethod != null)
                            {
                                rowMethod(reader);
                            }
                        }
                    }
                }
            }
        }

        #region Execute Non Scalar

        public int ExecuteNonQuery(string query, ParameterType[] parameters)
        {
            return ExecuteNonQuery(ConnectionStringSet, System.Data.CommandType.StoredProcedure, query, parameters);
        }

        public int ExecuteNonQuery(string connectionString, System.Data.CommandType commandType, string query, ParameterType[] parameters)
        {
            var rowsAffected = 0;

            using (var connection = new ConnectionType())
            {
                using (var command = new CommandType())
                {
                    command.Connection = connection;
                    command.CommandText = query;
                    command.CommandType = commandType;
                    if (parameters != null)
                    {
                        Array.ForEach(parameters, p => command.Parameters.Add(p));
                    }

                    connection.ConnectionString = connectionString;
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        #endregion
    }
}
