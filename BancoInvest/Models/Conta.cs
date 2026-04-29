namespace BancoInvest.Models
{
    public class Conta
    {
        public int Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Agencia { get; set; } = "0001";
        public decimal SaldoBRL { get; set; }
        public decimal SaldoUSD { get; set; }
        public decimal SaldoEUR { get; set; }
        public decimal SaldoGBP { get; set; }
        public decimal SaldoJPY { get; set; }
        public decimal LimiteEmprestimo { get; set; }
        public DateTime DataAbertura { get; set; } = DateTime.Now;
        public bool Ativa { get; set; } = true;
        public string UsuarioId { get; set; } = string.Empty;
        public Usuario? Usuario { get; set; }
        public ICollection<Transacao> TransacoesOrigem { get; set; } = new List<Transacao>();
        public ICollection<Transacao> TransacoesDestino { get; set; } = new List<Transacao>();
        public ICollection<Cartao> Cartoes { get; set; } = new List<Cartao>();
        public ICollection<Emprestimo> Emprestimos { get; set; } = new List<Emprestimo>();
        public ICollection<Cambio> Cambios { get; set; } = new List<Cambio>();
    }
}