namespace BancoInvest.Models
{
    public enum TipoTransacao
    {
        Deposito,
        Saque,
        TransferenciaEnviada,
        TransferenciaRecebida,
        Emprestimo,
        Cambio,
        Taxa
    }

    public class Transacao
    {
        public int Id { get; set; }
        public TipoTransacao Tipo { get; set; }
        public decimal Valor { get; set; }
        public decimal? Taxa { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataHora { get; set; } = DateTime.Now;
        public int? CambioId { get; set; }
        public Cambio? Cambio { get; set; }
        public int ContaOrigemId { get; set; }
        public Conta? ContaOrigem { get; set; }
        public int? ContaDestinoId { get; set; }
        public Conta? ContaDestino { get; set; }
    }
}