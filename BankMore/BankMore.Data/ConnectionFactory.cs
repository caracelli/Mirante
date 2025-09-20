using Microsoft.Data.Sqlite;
namespace BankMore.Data
{
    public interface IConnectionFactory
    {
        SqliteConnection CreateConnection();
    }

    public class SqliteConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;
        public SqliteConnectionFactory(string connectionString) => _connectionString = connectionString;
        public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);
    }
}
