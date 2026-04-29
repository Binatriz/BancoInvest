namespace BancoInvest.Models
{
    public class TipoCartao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal LimiteCredito { get; set; }
        public string Beneficios { get; set; } = string.Empty;
        public ICollection<Cartao> Cartoes { get; set; } = new List<Cartao>();
    }
}