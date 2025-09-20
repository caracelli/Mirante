using Microsoft.AspNetCore.Mvc;
using BankMore.Services;

namespace BankMore.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ContaController : ControllerBase
    {
        private readonly ContaService _service;
        public ContaController(ContaService service) => _service = service;

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromQuery] string cpf, [FromQuery] int numero)
        {
            await _service.CriarContaAsync(cpf, numero);
            return Ok("Conta criada com sucesso.");
        }

        [HttpGet("buscar/{cpf}")]
        public async Task<IActionResult> Buscar(string cpf)
        {
            var conta = await _service.BuscarContaPorCPF(cpf);
            if (conta == null) return NotFound();
            return Ok(conta);
        }
    }
}
