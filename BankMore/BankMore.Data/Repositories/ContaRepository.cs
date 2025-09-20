using Dapper;
using Microsoft.Data.Sqlite;
using BankMore.Domain.Models;

namespace BankMore.Data
{
    public interface IContaRepository
    {
        Task InsertContaAsync(ContaCorrente conta);
        Task<ContaCorrente?> GetByCPFAsync(string cpf);
        Task<ContaCorrente?> GetByNumeroAsync(int numero);
        Task UpdateAtivoAsync(string idConta, bool ativo);
        Task InsertMovimentoAsync(Movimento mov);
        Task<decimal> GetSaldoAsync(string idConta);
        Task InsertTransferenciaAsync(Transferencia t);
        Task<bool> ExistsContaByIdAsync(string id);
        Task InsertIdempotenciaAsync(string chave, string req, string res);
        Task<bool> ExistsIdempotenciaAsync(string chave);
        Task InsertTarifaAsync(string idtarifa, string idconta, decimal valor);
    }

    public class ContaRepository : IContaRepository
    {
        private readonly IConnectionFactory _factory;
        public ContaRepository(IConnectionFactory factory) => _factory = factory;

        public async Task InsertContaAsync(ContaCorrente conta)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = @"INSERT INTO contacorrente (idcontacorrente, numero, nome, cpf, senha, salt, ativo)
                        VALUES (@IdContaCorrente, @Numero, @Nome, @CPF, @Senha, @Salt, @Ativo)";
            await conn.ExecuteAsync(sql, conta);
        }

        public async Task<ContaCorrente?> GetByCPFAsync(string cpf)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = "SELECT * FROM contacorrente WHERE cpf = @cpf LIMIT 1";
            return await conn.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { cpf });
        }

        public async Task<ContaCorrente?> GetByNumeroAsync(int numero)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = "SELECT * FROM contacorrente WHERE numero = @numero LIMIT 1";
            return await conn.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { numero });
        }

        public async Task UpdateAtivoAsync(string idConta, bool ativo)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = "UPDATE contacorrente SET ativo = @ativo WHERE idcontacorrente = @id";
            await conn.ExecuteAsync(sql, new { id = idConta, ativo = ativo ? 1 : 0 });
        }

        public async Task InsertMovimentoAsync(Movimento mov)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = @"INSERT INTO movimento (idmovimento, idconta, datamovimento, tipomovimento, valor)
                        VALUES (@IdMovimento, @IdConta, @Datamovimento, @Tipo, @Valor)";
            await conn.ExecuteAsync(sql, mov);
        }

        public async Task<decimal> GetSaldoAsync(string idConta)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var creditos = await conn.ExecuteScalarAsync<decimal>("SELECT IFNULL(SUM(valor),0) FROM movimento WHERE idconta=@id AND tipomovimento='C'", new { id = idConta });
            var debitos = await conn.ExecuteScalarAsync<decimal>("SELECT IFNULL(SUM(valor),0) FROM movimento WHERE idconta=@id AND tipomovimento='D'", new { id = idConta });
            return creditos - debitos;
        }

        public async Task InsertTransferenciaAsync(Transferencia t)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = @"INSERT INTO transferencia (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
                        VALUES (@IdTransferencia, @IdContaOrigem, @IdContaDestino, @Datamovimento, @Valor)";
            await conn.ExecuteAsync(sql, new { IdTransferencia = t.IdTransferencia, IdContaOrigem = t.IdContaOrigem, IdContaDestino = t.IdContaDestino, Datamovimento = t.Datamovimento, Valor = t.Valor });
        }

        public async Task<bool> ExistsContaByIdAsync(string id)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM contacorrente WHERE idcontacorrente=@id";
            var c = await conn.ExecuteScalarAsync<int>(sql, new { id });
            return c > 0;
        }

        public async Task InsertIdempotenciaAsync(string chave, string req, string res)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = @"INSERT INTO idempotencia (chave_idempotencia, requisicao, resultado) VALUES (@chave, @req, @res)";
            await conn.ExecuteAsync(sql, new { chave, req, res });
        }

        public async Task<bool> ExistsIdempotenciaAsync(string chave)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = "SELECT COUNT(1) FROM idempotencia WHERE chave_idempotencia=@chave";
            var c = await conn.ExecuteScalarAsync<int>(sql, new { chave });
            return c > 0;
        }

        public async Task InsertTarifaAsync(string idtarifa, string idconta, decimal valor)
        {
            using var conn = _factory.CreateConnection();
            await conn.OpenAsync();
            var sql = @"INSERT INTO tarifa (idtarifa, idcontacorrente, datamovimento, valor) VALUES (@id, @idConta, @data, @valor)";
            await conn.ExecuteAsync(sql, new { id = idtarifa, idConta = idconta, data = DateTime.UtcNow.ToString("dd/MM/yyyy"), valor });
        }
    }
}
