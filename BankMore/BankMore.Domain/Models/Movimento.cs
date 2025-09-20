using System;
namespace BankMore.Domain.Models
{
    public class Movimento
    {
        public string IdMovimento { get; set; } = Guid.NewGuid().ToString();
        public string IdConta { get; set; } = string.Empty;
        public string Datamovimento { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public string Tipo { get; set; } = string.Empty; // C or D
        public decimal Valor { get; set; }
    }
}
