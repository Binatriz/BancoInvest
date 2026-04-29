using System.ComponentModel.DataAnnotations;

namespace BancoInvest.DTOs
{
    // 📥 INPUT DTOs
    public class AtualizarNomeDto
    {
        [Required]
        public string PrimeiroNome { get; set; } = string.Empty;

        [Required]
        public string Sobrenome { get; set; } = string.Empty;
    }

    public class DepositoDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
    }

    public class SaqueDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
    }

    public class TransferenciaDto
    {
        [Required]
        [EmailAddress]
        public string EmailDestino { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }
    }

    public class CambioDto
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Valor { get; set; }

        [Required]
        public string Moeda { get; set; } = string.Empty; // USD, EUR, GBP, JPY
    }

    public class EmprestimoDto
    {
        [Required]
        [Range(100, 100000)]
        public decimal Valor { get; set; }

        [Required]
        [Range(1, 48)]
        public int Parcelas { get; set; }
    }

    // 📤 OUTPUT DTOs
    public class TransactionResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public decimal NovoSaldo { get; set; }
    }

    public class CambioResponseDto
    {
        public decimal ValorOriginal { get; set; }
        public string MoedaDestino { get; set; } = string.Empty;
        public decimal Taxa { get; set; }
        public decimal ValorConvertido { get; set; }
    }

    // 👤 USUÁRIO
    public class UsuarioDto
    {
        public string Id { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public bool Ativo { get; set; }

        public ContaResumoDto? ContaResumida { get; set; }
    }

    public class ContaResumoDto
    {
        public int Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public decimal SaldoBRL { get; set; }
    }

    // 💳 CARTÃO
    public class CartaoDto
    {
        public int Id { get; set; }
        public string FinalCartao { get; set; } = string.Empty;
        public string Bandeira { get; set; } = string.Empty;
        public DateTime Validade { get; set; }
        public decimal? LimiteDisponivel { get; set; } // null para débito
        public bool Ativo { get; set; }
        public string Tipo { get; set; } = string.Empty; // Crédito ou Débito
    }

    // 💰 EMPRÉSTIMO
    public class EmprestimoResumoDto
    {
        public int Id { get; set; }
        public decimal ValorAprovado { get; set; }
        public int NumeroParcelas { get; set; }
        public decimal TaxaJuros { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DataAprovacao { get; set; }
    }

    // 💱 CÂMBIO
    public class CambioResumoDto
    {
        public int Id { get; set; }
        public string MoedaDestino { get; set; } = string.Empty;
        public decimal ValorConvertido { get; set; }
        public decimal TaxaCambio { get; set; }
        public DateTime DataHora { get; set; }
    }

    // 🏦 CONTA COMPLETA
    public class ContaDetalhesDto
    {
        public int Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Agencia { get; set; } = "0001";

        public string Email { get; set; } = string.Empty;
        public decimal SaldoBRL { get; set; }
        public decimal SaldoUSD { get; set; }
        public decimal SaldoEUR { get; set; }
        public decimal SaldoGBP { get; set; }
        public decimal SaldoJPY { get; set; }

        public decimal LimiteEmprestimo { get; set; }
        public DateTime DataAbertura { get; set; }
        public bool Ativa { get; set; }

        public UsuarioDto? Usuario { get; set; }

        public ICollection<CartaoDto> Cartoes { get; set; } = new List<CartaoDto>();
        public ICollection<EmprestimoResumoDto> Emprestimos { get; set; } = new List<EmprestimoResumoDto>();
        public ICollection<CambioResumoDto> Cambios { get; set; } = new List<CambioResumoDto>();
    }
}