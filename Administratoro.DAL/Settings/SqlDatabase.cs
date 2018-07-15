
namespace Administratoro.DAL.Settings
{
    using System;
    using System.Data;

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

        #endregion

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
