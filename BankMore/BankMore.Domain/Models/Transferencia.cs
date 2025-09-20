using System;
namespace BankMore.Domain.Models
{
    public class Transferencia
    {
        public string IdTransferencia { get; set; } = Guid.NewGuid().ToString();
        public string IdContaOrigem { get; set; } = string.Empty;
        public string IdContaDestino { get; set; } = string.Empty;
        public string Datamovimento { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public decimal Valor { get; set; }
    }
}
