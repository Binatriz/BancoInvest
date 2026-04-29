using BancoInvest.Data;
using BancoInvest.DTOs;
using BancoInvest.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BancoInvest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ContaApiController : ControllerBase
    {
        private readonly BancoInvestContext _ctx;

        public ContaApiController(BancoInvestContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet("debug-user")]
        public async Task<IActionResult> DebugUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var usuario = await _ctx.Users.FindAsync(userId);

            return Ok(new
            {
                userId,
                userName = usuario?.NomeCompleto,
                email
            });
        }

        private async Task<Conta?> GetContaLogada()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            return await _ctx.Contas
                .Include(c => c.Usuario)
                .Include(c => c.Cartoes)
                    .ThenInclude(c => c.TipoCartao) // 🔥 IMPORTANTE
                .Include(c => c.Emprestimos)
                .Include(c => c.Cambios)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);
        }

        // 🏦 GET CONTA (DTO )
        [HttpGet]
        public async Task<ActionResult<ContaDetalhesDto>> GetConta()
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            var dto = new ContaDetalhesDto
            {
                Id = conta.Id,
                NumeroConta = conta.NumeroConta,
                Agencia = conta.Agencia,
                Email = conta.Usuario.Email,
                SaldoBRL = conta.SaldoBRL,
                SaldoUSD = conta.SaldoUSD,
                SaldoEUR = conta.SaldoEUR,
                SaldoGBP = conta.SaldoGBP,
                SaldoJPY = conta.SaldoJPY,
                LimiteEmprestimo = conta.LimiteEmprestimo,
                DataAbertura = conta.DataAbertura,
                Ativa = conta.Ativa,

                Usuario = conta.Usuario == null ? null : new UsuarioDto
                {
                    Id = conta.Usuario.Id,
                    NomeCompleto = conta.Usuario.NomeCompleto,
                    DataCadastro = conta.Usuario.DataCadastro,
                    Ativo = conta.Usuario.Ativo,
                    ContaResumida = new ContaResumoDto
                    {
                        Id = conta.Id,
                        NumeroConta = conta.NumeroConta,
                        SaldoBRL = conta.SaldoBRL
                    }
                },

                Cartoes = conta.Cartoes.Select(c => new CartaoDto
                {
                    Id = c.Id,
                    FinalCartao = c.FinalCartao,
                    Bandeira = c.Bandeira,
                    Validade = c.Validade,
                    LimiteDisponivel = c.TipoCartao.Nome == "Crédito" ? c.LimiteDisponivel : null,
                    Ativo = c.Ativo,
                    Tipo = c.TipoCartao.Nome
                }).ToList(),

                Emprestimos = conta.Emprestimos.Select(e => new EmprestimoResumoDto
                {
                    Id = e.Id,
                    ValorAprovado = e.ValorAprovado,
                    NumeroParcelas = e.NumeroParcelas,
                    TaxaJuros = e.TaxaJuros,
                    Status = e.Status.ToString(),
                    DataAprovacao = e.DataAprovacao ?? DateTime.MinValue
                }).ToList(),

                Cambios = conta.Cambios.Select(c => new CambioResumoDto
                {
                    Id = c.Id,
                    MoedaDestino = c.MoedaDestino,
                    ValorConvertido = c.ValorConvertido,
                    TaxaCambio = c.TaxaCambio,
                    DataHora = c.DataHora
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut("atualizar-nome")]
        public async Task<IActionResult> AtualizarNome([FromBody] AtualizarNomeDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PrimeiroNome) || string.IsNullOrWhiteSpace(dto.Sobrenome))
                return BadRequest("Nome inválido");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var usuario = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return NotFound("Usuário não encontrado");

            usuario.NomeCompleto = $"{dto.PrimeiroNome.Trim()} {dto.Sobrenome.Trim()}";

            await _ctx.SaveChangesAsync();

            return Ok(new
            {
                message = "Nome atualizado com sucesso",
                nome = usuario.NomeCompleto
            });
        }

        // 💰 DEPÓSITO
        [HttpPost("depositar")]
        public async Task<ActionResult<TransactionResponseDto>> Depositar([FromBody] DepositoDto dto)
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            conta.SaldoBRL += dto.Valor;

            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Deposito,
                Valor = dto.Valor,
                ContaOrigemId = conta.Id,
                DataHora = DateTime.Now
            });

            await _ctx.SaveChangesAsync();

            return Ok(new TransactionResponseDto
            {
                Message = "Depósito realizado para {conta.Usuario.Email}",
                NovoSaldo = conta.SaldoBRL
            });
        }

        // 🏧 SAQUE
        [HttpPost("sacar")]
        public async Task<ActionResult<TransactionResponseDto>> Sacar([FromBody] SaqueDto dto)
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            if (conta.SaldoBRL < dto.Valor + 2.5m)
                return BadRequest("Saldo insuficiente");

            conta.SaldoBRL -= (dto.Valor + 2.5m);

            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Saque,
                Valor = dto.Valor,
                Taxa = 2.5m,
                ContaOrigemId = conta.Id,
                DataHora = DateTime.Now
            });

            await _ctx.SaveChangesAsync();

            return Ok(new TransactionResponseDto
            {
                Message = "Saque realizado",
                NovoSaldo = conta.SaldoBRL
            });
        }

        // 🔁 TRANSFERÊNCIA
        [HttpPost("transferir")]
        public async Task<IActionResult> Transferir([FromBody] TransferenciaDto dto)
        {
            var origem = await GetContaLogada();
            if (origem == null) return Unauthorized();

            var destino = await _ctx.Contas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.Usuario.Email == dto.EmailDestino);

            if (destino == null)
                return NotFound("Destinatário não encontrado");

            if (origem.SaldoBRL < dto.Valor)
                return BadRequest("Saldo insuficiente");

            origem.SaldoBRL -= dto.Valor;
            destino.SaldoBRL += dto.Valor;

            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.TransferenciaEnviada,
                Valor = dto.Valor,
                ContaOrigemId = origem.Id,
                ContaDestinoId = destino.Id,
                Descricao = $"Transferência para {destino.Usuario.NomeCompleto}",
                DataHora = DateTime.Now
            });

            await _ctx.SaveChangesAsync();

            return Ok(new { message = "Transferência concluída" });
        }

        // 📄 EXTRATO (SIMPLIFICADO)
        [HttpGet("extrato")]
        public async Task<IActionResult> Extrato()
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            var transacoes = await _ctx.Transacoes
                .Where(t => t.ContaOrigemId == conta.Id || t.ContaDestinoId == conta.Id)
                .OrderByDescending(t => t.DataHora)
                .Select(t => new
                {
                    t.Id,
                    t.Tipo,
                    t.Valor,
                    t.Taxa,
                    t.Descricao,
                    t.DataHora
                })
                .ToListAsync();

            return Ok(transacoes);
        }

        // 💱 CÂMBIO (SIMULAÇÃO)
        [HttpPost("cambio")]
        public ActionResult<CambioResponseDto> Cambio([FromBody] CambioDto dto)
        {
            decimal taxa = dto.Moeda switch
            {
                "USD" => 5.0m,
                "EUR" => 5.4m,
                "GBP" => 6.2m,
                "JPY" => 0.035m,
                _ => 0
            };

            if (taxa == 0)
                return BadRequest("Moeda não suportada");

            return Ok(new CambioResponseDto
            {
                ValorOriginal = dto.Valor,
                MoedaDestino = dto.Moeda,
                Taxa = taxa,
                ValorConvertido = Math.Round(dto.Valor / taxa, 2) // 🔥 evita erro decimal
            });
        }

        [HttpGet("cartoes")]
        public async Task<ActionResult<IEnumerable<CartaoDto>>> GetCartoes()
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            var cartoes = conta.Cartoes.Select(c => new CartaoDto
            {
                Id = c.Id,
                FinalCartao = c.FinalCartao,
                Bandeira = c.Bandeira,
                Validade = c.Validade,
                LimiteDisponivel = c.TipoCartao.Nome == "Crédito" ? c.LimiteDisponivel : null,
                Ativo = c.Ativo,
                Tipo = c.TipoCartao.Nome
            }).ToList();

            return Ok(cartoes);
        }

        [HttpDelete("cartoes/{cartaoId}")]
        public async Task<IActionResult> ExcluirCartao(int cartaoId)
        {
            var conta = await GetContaLogada();
            if (conta == null) return Unauthorized();

            var cartao = await _ctx.Cartoes
                .FirstOrDefaultAsync(c => c.Id == cartaoId && c.ContaId == conta.Id);

            if (cartao == null)
                return NotFound("Cartão não encontrado ou não pertence ao usuário");

            _ctx.Cartoes.Remove(cartao);
            await _ctx.SaveChangesAsync();

            return Ok(new { message = "Cartão excluído com sucesso" });
        }
    }
}