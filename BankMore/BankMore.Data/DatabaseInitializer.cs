using Microsoft.Data.Sqlite;
namespace BankMore.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS contacorrente (
                    idcontacorrente TEXT PRIMARY KEY,
                    numero INTEGER NOT NULL UNIQUE,
                    nome TEXT NOT NULL,
                    cpf TEXT NOT NULL,
                    senha TEXT NOT NULL,
                    salt TEXT NOT NULL,
                    ativo INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS movimento (
                    idmovimento TEXT PRIMARY KEY,
                    idconta TEXT NOT NULL,
                    datamovimento TEXT NOT NULL,
                    tipomovimento TEXT NOT NULL,
                    valor REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS transferencia (
                    idtransferencia TEXT PRIMARY KEY,
                    idcontacorrente_origem TEXT NOT NULL,
                    idcontacorrente_destino TEXT NOT NULL,
                    datamovimento TEXT NOT NULL,
                    valor REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS idempotencia (
                    chave_idempotencia TEXT PRIMARY KEY,
                    requisicao TEXT,
                    resultado TEXT
                );

                CREATE TABLE IF NOT EXISTS tarifa (
                    idtarifa TEXT PRIMARY KEY,
                    idcontacorrente TEXT NOT NULL,
                    datamovimento TEXT NOT NULL,
                    valor REAL NOT NULL
                );
            ";
            cmd.ExecuteNonQuery();
        }
    }
}
