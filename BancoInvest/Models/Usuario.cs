using Microsoft.AspNetCore.Identity;

namespace BancoInvest.Models
{
    public class Usuario : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
        public Conta? Conta { get; set; }
    }
}