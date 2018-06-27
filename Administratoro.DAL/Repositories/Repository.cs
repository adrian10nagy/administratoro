
namespace Administratoro.DAL.Repositories
{
    using DAL.Settings;
    using System;
    using System.Data;
    using System.Data.SqlClient;

    public interface IRepository
    {

    }

    public partial class Repository : IRepository
    {
        private SqlDatabase<SqlConnection, SqlCommand, SqlParameter> _dbRead = new SqlDatabase<SqlConnection, SqlCommand, SqlParameter>();
        private SqlDatabase<SqlConnection, SqlCommand, SqlParameter> _dbWrite = new SqlDatabase<SqlConnection, SqlCommand, SqlParameter>();

        public static T Read<T>(IDataRecord record, string key)
        {
            return Read(record, key, default(T));
        }

        public static T Read<T>(IDataRecord record, string key, T defaultValue)
        {
            return record[key] != DBNull.Value ? (T)record[key] : defaultValue;
        }

    }
}
