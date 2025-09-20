using BankMore.Domain.Models;
using BankMore.Data;

namespace BankMore.Services
{
    public class ContaService
    {
        private readonly IContaRepository _repo;
        public ContaService(IContaRepository repo) => _repo = repo;

        public async Task CriarContaAsync(string cpf, int numero)
        {
            var conta = new ContaCorrente { CPF = cpf, Numero = numero };
            await _repo.InsertContaAsync(conta);
        }

        public async Task<ContaCorrente?> BuscarContaPorCPF(string cpf) =>
            await _repo.GetByCPFAsync(cpf);

        public async Task TransferirAsync(string idOrigem, string idDestino, decimal valor)
        {
            var saldoOrigem = await _repo.GetSaldoAsync(idOrigem);
            if (saldoOrigem < valor) throw new Exception("Saldo insuficiente.");

            await _repo.InsertMovimentoAsync(new Movimento { IdConta = idOrigem, Valor = -valor, Tipo = "D" });
            await _repo.InsertMovimentoAsync(new Movimento { IdConta = idDestino, Valor = valor, Tipo = "C" });

            await _repo.InsertTransferenciaAsync(new Transferencia
            {
                IdContaOrigem = idOrigem,
                IdContaDestino = idDestino,
                Valor = valor
            });
        }
    }
}
