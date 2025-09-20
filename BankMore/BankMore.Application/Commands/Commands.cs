using MediatR;
using BankMore.Domain.Models;

namespace BankMore.Application.Commands
{
    public record CriarContaCommand(string CPF, string Nome, string Senha) : IRequest<ContaCorrente>;
    public record LoginCommand(string CPFOuNumero, string Senha) : IRequest<string>;
    public record MovimentacaoCommand(string IdRequisicao, int? NumeroConta, decimal Valor, string Tipo, string? IdTokenConta) : IRequest;
    public record TransferenciaCommand(string IdRequisicao, int NumeroContaDestino, decimal Valor, string ContaOrigemId) : IRequest;
}
