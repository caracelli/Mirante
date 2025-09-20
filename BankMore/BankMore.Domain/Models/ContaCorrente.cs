using System;
namespace BankMore.Domain.Models
{
    public class ContaCorrente
    {
        public string IdContaCorrente { get; set; } = Guid.NewGuid().ToString();
        public int Numero { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
    }
}
