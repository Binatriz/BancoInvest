namespace BancoInvest.Models
{
    public enum StatusEmprestimo { Pendente, Aprovado, Negado, Quitado }

    public class Emprestimo
    {
        public int Id { get; set; }
        public decimal ValorSolicitado { get; set; }
        public decimal ValorAprovado { get; set; }
        public decimal TaxaJuros { get; set; } = 2.5m;
        public int NumeroParcelas { get; set; }
        public StatusEmprestimo Status { get; set; } = StatusEmprestimo.Pendente;
        public DateTime DataSolicitacao { get; set; } = DateTime.Now;
        public DateTime? DataAprovacao { get; set; }
        public int ContaId { get; set; }
        public Conta? Conta { get; set; }
    }
}