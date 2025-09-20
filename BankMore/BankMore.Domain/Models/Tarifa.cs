using System;
namespace BankMore.Domain.Models
{
    public class Tarifa
    {
        public string IdTarifa { get; set; } = Guid.NewGuid().ToString();
        public string IdConta { get; set; } = string.Empty;
        public string Datamovimento { get; set; } = DateTime.UtcNow.ToString("dd/MM/yyyy");
        public decimal Valor { get; set; }
    }
}
