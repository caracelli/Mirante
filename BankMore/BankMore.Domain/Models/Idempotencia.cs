﻿namespace BankMore.Domain.Models
{
    public class Idempotencia
    {
        public string ChaveIdempotencia { get; set; } = string.Empty;
        public string Requisicao { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
    }
}
