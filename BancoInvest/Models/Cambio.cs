namespace BancoInvest.Models
{
    public class Cambio
    {
        public int Id { get; set; }
        public string MoedaOrigem { get; set; } = "BRL";
        public string MoedaDestino { get; set; } = string.Empty;
        public decimal ValorOrigem { get; set; }
        public decimal ValorConvertido { get; set; }
        public decimal TaxaCambio { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
    }
}