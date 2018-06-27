
namespace Administratoro.DAL.Settings
{
    using System.Data.SqlClient;

    public static class Configure
    {
        private static string _connectionString = System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];

        public static string ConnectionString
        {
            get { return _connectionString; }
            private set { _connectionString = value; }
        }

        private static SqlConnection _SqlConnection = new SqlConnection(ConnectionString);
        public static SqlConnection SQLConnection
        {
            get { return _SqlConnection; }
            private set { _SqlConnection = value; }
        }
    }
}
