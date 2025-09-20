using Microsoft.Data.Sqlite;
using BankMore.Domain.Models;

namespace BankMore.Data
{
    public static class Database
    {
        private static string _connectionString = "Data Source=BankMore.sqlite";

        public static void Initialize()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS ContaCorrente (
                IdContaCorrente TEXT PRIMARY KEY,
                Numero INTEGER NOT NULL,
                Nome TEXT NOT NULL,
                Senha TEXT NOT NULL,
                Salt TEXT NOT NULL,
                Ativo INTEGER NOT NULL
            );";
            cmd.ExecuteNonQuery();
        }

        public static void InsertConta(ContaCorrente conta)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO ContaCorrente (IdContaCorrente, Numero, Nome, Senha, Salt, Ativo)
                VALUES (, , , , , )";
            cmd.Parameters.AddWithValue("", conta.IdContaCorrente);
            cmd.Parameters.AddWithValue("", conta.Numero);
            cmd.Parameters.AddWithValue("", conta.Nome);
            cmd.Parameters.AddWithValue("", conta.Senha);
            cmd.Parameters.AddWithValue("", conta.Salt);
            cmd.Parameters.AddWithValue("", conta.Ativo ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        public static ContaCorrente? GetContaByNome(string nome)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM ContaCorrente WHERE Nome = ";
            cmd.Parameters.AddWithValue("", nome);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new ContaCorrente
                {
                    IdContaCorrente = reader.GetString(0),
                    Numero = reader.GetInt32(1),
                    Nome = reader.GetString(2),
                    Senha = reader.GetString(3),
                    Salt = reader.GetString(4),
                    Ativo = reader.GetInt32(5) == 1
                };
            }
            return null;
        }
    }
}
