using BancoInvest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BancoInvest.Data
{
    public class BancoInvestContext : IdentityDbContext<Usuario>
    {
        public BancoInvestContext(DbContextOptions<BancoInvestContext> options)
            : base(options) { }

        public DbSet<Conta> Contas { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Cartao> Cartoes { get; set; }
        public DbSet<TipoCartao> TiposCartao { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<Cambio> Cambios { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Conta → Usuario
            builder.Entity<Conta>()
                .HasOne(c => c.Usuario)
                .WithOne(u => u.Conta)
                .HasForeignKey<Conta>(c => c.UsuarioId);

            // Transação → Conta Origem
            builder.Entity<Transacao>()
                .HasOne(t => t.ContaOrigem)
                .WithMany(c => c.TransacoesOrigem)
                .HasForeignKey(t => t.ContaOrigemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transacao>()
                .HasOne(t => t.Cambio)
                .WithMany()
                .HasForeignKey(t => t.CambioId);

            // Transação → Conta Destino
            builder.Entity<Transacao>()
                .HasOne(t => t.ContaDestino)
                .WithMany(c => c.TransacoesDestino)
                .HasForeignKey(t => t.ContaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Cambio>()
            .Property(c => c.ValorConvertido)
            .HasColumnType("decimal(18,6)");

            // CONTA
            builder.Entity<Conta>()
                .Property(c => c.SaldoBRL)
                .HasColumnType("decimal(18,2)");

            // CONTAS - MOEDAS
            builder.Entity<Conta>()
                .Property(c => c.SaldoUSD)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Conta>()
                .Property(c => c.SaldoEUR)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Conta>()
                .Property(c => c.SaldoGBP)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Conta>()
                .Property(c => c.SaldoJPY)
                .HasColumnType("decimal(18,0)");

            // TRANSAÇÃO
            builder.Entity<Transacao>()
                .Property(t => t.Valor)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Transacao>()
                .Property(t => t.Taxa)
                .HasColumnType("decimal(18,2)");

            // CARTÃO
            builder.Entity<Cartao>()
                .Property(c => c.LimiteDisponivel)
                .HasColumnType("decimal(18,2)");

            // EMPRÉSTIMO
            builder.Entity<Emprestimo>()
                .Property(e => e.ValorSolicitado)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Emprestimo>()
                .Property(e => e.ValorAprovado)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Emprestimo>()
                .Property(e => e.TaxaJuros)
                .HasColumnType("decimal(18,2)");

            // CÂMBIO
            builder.Entity<Cambio>()
                .Property(c => c.ValorOrigem)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Cambio>()
                .Property(c => c.ValorConvertido)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Cambio>()
                .Property(c => c.TaxaCambio)
                .HasColumnType("decimal(18,4)"); 

            // TIPO CARTÃO
            builder.Entity<TipoCartao>()
                .Property(t => t.LimiteCredito)
                .HasColumnType("decimal(18,2)");
        }
    }
}