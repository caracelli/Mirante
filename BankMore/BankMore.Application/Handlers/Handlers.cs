using Microsoft.Extensions.Configuration;
using MediatR;
using BankMore.Application.Commands;
using BankMore.Domain.Models;
using BankMore.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankMore.Application.Handlers
{
    public class CriarContaHandler : IRequestHandler<CriarContaCommand, ContaCorrente>
    {
        private readonly IContaRepository _repo;
        public CriarContaHandler(IContaRepository repo) => _repo = repo;

        public async Task<ContaCorrente> Handle(CriarContaCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.CPF) || request.CPF.Length != 11)
                throw new ArgumentException("CPF invalido", "CPF");

            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Senha + salt)));

            var conta = new ContaCorrente
            {
                IdContaCorrente = Guid.NewGuid().ToString(),
                Numero = new Random().Next(100000, 999999),
                Nome = request.Nome,
                CPF = request.CPF,
                Senha = hash,
                Salt = salt,
                Ativo = true
            };

            await _repo.InsertContaAsync(conta);
            return conta;
        }
    }

    public class LoginHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IContaRepository _repo;
        private readonly IConfiguration _config;
        public LoginHandler(IContaRepository repo, IConfiguration config) { _repo = repo; _config = config; }

        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ContaCorrente? conta = null;
            if (int.TryParse(request.CPFOuNumero, out var numero))
            {
                conta = await _repo.GetByNumeroAsync(numero);
            }
            else
            {
                conta = await _repo.GetByCPFAsync(request.CPFOuNumero);
            }

            if (conta == null) throw new UnauthorizedAccessException("USER_UNAUTHORIZED");

            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(request.Senha + conta.Salt)));
            if (hash != conta.Senha) throw new UnauthorizedAccessException("USER_UNAUTHORIZED");

            var jwtCfg = _config.GetSection("Jwt");
            var secret = jwtCfg.GetValue<string>("Secret") ?? "change_this_secret";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim("idconta", conta.IdContaCorrente),
                new Claim("numero", conta.Numero.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtCfg.GetValue<string>("Issuer"),
                audience: jwtCfg.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class MovimentacaoHandler : IRequestHandler<MovimentacaoCommand>
    {
        private readonly IContaRepository _repo;
        public MovimentacaoHandler(IContaRepository repo) { _repo = repo; }

        public async Task<Unit> Handle(MovimentacaoCommand request, CancellationToken cancellationToken)
        {
            if (request.Valor <= 0) throw new ArgumentException("Valor deve ser positivo", "INVALID_VALUE");
            if (request.Tipo != "C" && request.Tipo != "D") throw new ArgumentException("Tipo invalido", "INVALID_TYPE");

            // Determine idConta based on token or provided number
            string? idConta = request.IdTokenConta;
            if (string.IsNullOrEmpty(idConta) && request.NumeroConta.HasValue)
            {
                var c = await _repo.GetByNumeroAsync(request.NumeroConta.Value);
                idConta = c?.IdContaCorrente;
            }
            if (string.IsNullOrEmpty(idConta)) throw new ArgumentException("Conta invalida", "INVALID_ACCOUNT");

            var contaExists = await _repo.ExistsContaByIdAsync(idConta);
            if (!contaExists) throw new ArgumentException("Conta invalida", "INVALID_ACCOUNT");

            // Ideally check Ativo (requires repository method to return account with Ativo) - simplified as Exists
            var mov = new Movimento
            {
                IdMovimento = Guid.NewGuid().ToString(),
                IdConta = idConta,
                Datamovimento = DateTime.UtcNow.ToString("dd/MM/yyyy"),
                Tipo = request.Tipo,
                Valor = request.Valor
            };

            await _repo.InsertMovimentoAsync(mov);
            return Unit.Value;
        }
    }

    public class TransferenciaHandler : IRequestHandler<TransferenciaCommand>
    {
        private readonly IContaRepository _repo;
        public TransferenciaHandler(IContaRepository repo) { _repo = repo; }

        public async Task<Unit> Handle(TransferenciaCommand request, CancellationToken cancellationToken)
        {
            var chave = request.IdRequisicao;
            if (string.IsNullOrWhiteSpace(chave)) throw new ArgumentException("id requisicao obrigatorio", "INVALID_VALUE");
            if (await _repo.ExistsIdempotenciaAsync(chave)) return Unit.Value;

            var destino = await _repo.GetByNumeroAsync(request.NumeroContaDestino);
            if (destino == null) throw new ArgumentException("Conta destino invalida", "INVALID_ACCOUNT");

            if (!await _repo.ExistsContaByIdAsync(request.ContaOrigemId)) throw new ArgumentException("Conta origem invalida", "INVALID_ACCOUNT");
            if (request.Valor <= 0) throw new ArgumentException("Valor invalido", "INVALID_VALUE");

            // DÃ©bito origem
            var debito = new Movimento { IdMovimento = Guid.NewGuid().ToString(), IdConta = request.ContaOrigemId, Datamovimento = DateTime.UtcNow.ToString("dd/MM/yyyy"), Tipo = "D", Valor = request.Valor };
            await _repo.InsertMovimentoAsync(debito);

            // CrÃ©dito destino
            var credito = new Movimento { IdMovimento = Guid.NewGuid().ToString(), IdConta = destino.IdContaCorrente, Datamovimento = DateTime.UtcNow.ToString("dd/MM/yyyy"), Tipo = "C", Valor = request.Valor };
            await _repo.InsertMovimentoAsync(credito);

            // Persist transferÃªncia
            var t = new Transferencia { IdContaOrigem = request.ContaOrigemId, IdContaDestino = destino.IdContaCorrente, Valor = request.Valor, Datamovimento = DateTime.UtcNow.ToString("dd/MM/yyyy") };
            await _repo.InsertTransferenciaAsync(t);

            // registra idempotencia
            await _repo.InsertIdempotenciaAsync(chave, $"orig:{request.ContaOrigemId};dest:{destino.IdContaCorrente};val:{request.Valor}", "SUCESSO");

            return Unit.Value;
        }
    }
}
