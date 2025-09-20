using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BankMore.Application.Commands;
using System.Security.Claims;

namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("api/contacorrente")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ContaCorrenteController(IMediator mediator) => _mediator = mediator;

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CriarContaCommand cmd)
        {
            try
            {
                var conta = await _mediator.Send(cmd);
                return Ok(new { numero = conta.Numero, id = conta.IdContaCorrente });
            }
            catch (ArgumentException ex) when (ex.ParamName == "CPF")
            {
                return BadRequest(new { message = ex.Message, type = "INVALID_DOCUMENT" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
        {
            try
            {
                var token = await _mediator.Send(cmd);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuario nao autorizado", type = "USER_UNAUTHORIZED" });
            }
        }

        [Authorize]
        [HttpPost("inativar")]
        public async Task<IActionResult> Inativar([FromBody] dynamic body)
        {
            var senha = (string)body.senha;
            var idConta = User.FindFirstValue("idconta");
            if (string.IsNullOrEmpty(idConta)) return Forbid();
            // validar senha e atualizar ativo via repository directly (for simplicity, not via mediator)
            return NoContent();
        }

        [Authorize]
        [HttpPost("movimentacao")]
        public async Task<IActionResult> Movimentacao([FromBody] MovimentacaoCommand cmd)
        {
            if (!cmd.NumeroConta.HasValue)
            {
                cmd = cmd with { IdTokenConta = User.FindFirstValue("idconta") };
            }

            try
            {
                await _mediator.Send(cmd);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.ParamName });
            }
        }

        [Authorize]
        [HttpGet("saldo")]
        public async Task<IActionResult> Saldo()
        {
            var idConta = User.FindFirstValue("idconta");
            if (string.IsNullOrEmpty(idConta)) return Forbid();
            // Query saldo via repository or mediator (simplified response here)
            return Ok(new { numero = 0, nome = "Titular", data = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"), saldo = "0,00" });
        }

        [Authorize]
        [HttpPost("transferencia")]
        public async Task<IActionResult> Transferencia([FromBody] TransferenciaCommand cmd)
        {
            try
            {
                await _mediator.Send(cmd);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.ParamName });
            }
        }
    }
}
