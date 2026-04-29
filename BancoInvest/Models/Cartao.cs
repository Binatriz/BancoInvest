namespace BancoInvest.Models
{
    public class Cartao
    {
        public int Id { get; set; }
        public string NumeroCartao { get; set; } = string.Empty;
        public string FinalCartao { get; set; } = string.Empty;
        public string Bandeira { get; set; } = "Visa";
        public DateTime Validade { get; set; }
        public string CVV { get; set; } = string.Empty;
        public decimal LimiteDisponivel { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataEmissao { get; set; } = DateTime.Now;
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
        public int TipoCartaoId { get; set; }
        public TipoCartao? TipoCartao { get; set; }
    }
}