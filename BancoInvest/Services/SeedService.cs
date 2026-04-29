using BancoInvest.Data;
using BancoInvest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BancoInvest.Services
{
    public class SeedService
    {
        private readonly BancoInvestContext _ctx;
        private readonly UserManager<Usuario> _userManager;

        public SeedService(BancoInvestContext ctx, UserManager<Usuario> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await SeedTiposCartao();
            await SeedUsuarios();
        }

        // 💳 TIPOS DE CARTÃO
        private async Task SeedTiposCartao()
        {
            if (!await _ctx.TiposCartao.AnyAsync())
            {
                _ctx.TiposCartao.AddRange(
                    new TipoCartao
                    {
                        Nome = "Crédito",
                        LimiteCredito = 2000,
                        Beneficios = "Compras parceladas, cashback"
                    },
                    new TipoCartao
                    {
                        Nome = "Débito",
                        LimiteCredito = 0,
                        Beneficios = "Uso direto do saldo da conta"
                    }
                );

                await _ctx.SaveChangesAsync();
            }
        }

        // 👤 USUÁRIOS
        private async Task SeedUsuarios()
        {
            await CriarUsuarioComConta(
                "teste1@teste.com",
                "Usuário 1",
                "12345-6",
                5000
            );

            await CriarUsuarioComConta(
                "teste2@teste.com",
                "Usuário 2",
                "99999-9",
                3000
            );
        }

        // 🏦 CONTA + USUÁRIO
        private async Task CriarUsuarioComConta(string email, string nome, string numeroConta, decimal saldo)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new Usuario
                {
                    UserName = email,
                    Email = email,
                    NomeCompleto = nome,
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, "Admin@123");

                if (!result.Succeeded)
                    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            user = await _userManager.FindByEmailAsync(email);

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == user.Id);

            if (conta == null)
            {
                conta = new Conta
                {
                    NumeroConta = numeroConta,
                    Agencia = "0001",
                    SaldoBRL = saldo,
                    UsuarioId = user.Id,
                    LimiteEmprestimo = 50000,
                    DataAbertura = DateTime.Now,
                    Ativa = true
                };

                _ctx.Contas.Add(conta);

                // 💰 transação inicial
                _ctx.Transacoes.Add(new Transacao
                {
                    Tipo = TipoTransacao.Deposito,
                    Valor = saldo,
                    ContaOrigem = conta,
                    Descricao = "Saldo inicial",
                    DataHora = DateTime.Now
                });

                await _ctx.SaveChangesAsync();
            }
        }
    }
}