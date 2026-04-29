using BancoInvest.Data;
using BancoInvest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace BancoInvest.Controllers
{
    public class ContaController : Controller
    {
        private readonly BancoInvestContext _ctx;

        public ContaController(BancoInvestContext ctx)
        {
            _ctx = ctx;
        }

        // 🔒 PROTEÇÃO DE LOGIN
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = RedirectToAction("Login", "Auth");
            }

            base.OnActionExecuting(context);
        }

        // 📊 DASHBOARD
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .Include(c => c.Usuario)
                .Include(c => c.Cartoes)
                .ThenInclude(c => c.TipoCartao)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View(conta);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarNome(string primeiroNome, string sobrenome)
        {
            if (string.IsNullOrWhiteSpace(primeiroNome) || string.IsNullOrWhiteSpace(sobrenome))
            {
                TempData["Erro"] = "Nome não pode ser vazio";
                return RedirectToAction("Index");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var usuario = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            usuario.NomeCompleto = $"{primeiroNome.Trim()} {sobrenome.Trim()}";

            await _ctx.SaveChangesAsync();

            TempData["Sucesso"] = "Nome atualizado com sucesso";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarCartao(string bandeira, int tipoCartaoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            var tipoCartao = await _ctx.TiposCartao
            .FirstOrDefaultAsync(t => t.Id == tipoCartaoId);

            decimal limite = 0;

            // regra de negócio
            if (tipoCartao.Nome == "Crédito")
            {
                limite = 2000; // ou baseado no usuário depois
            }

            var random = new Random();

            var numero = string.Join("", Enumerable.Range(0, 16)
                .Select(_ => random.Next(0, 10)));

            var final = numero.Substring(numero.Length - 4);

            var cartao = new Cartao
            {
                NumeroCartao = numero,
                FinalCartao = final,
                Bandeira = bandeira,
                Validade = DateTime.Now.AddYears(5),
                CVV = random.Next(100, 999).ToString(),
                LimiteDisponivel = tipoCartao.LimiteCredito,
                ContaId = conta.Id,
                Ativo = true,
                TipoCartaoId = tipoCartao.Id
            };

            _ctx.Cartoes.Add(cartao);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> ExcluirCartao(int cartaoId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var cartao = await _ctx.Cartoes
                .Include(c => c.Conta)
                .FirstOrDefaultAsync(c => c.Id == cartaoId && c.Conta.UsuarioId == userId);

            if (cartao == null)
                return RedirectToAction("Index");

            _ctx.Cartoes.Remove(cartao);
            await _ctx.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 💰 DEPOSITAR
        [HttpPost]
        public async Task<IActionResult> Depositar(decimal valor)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null) return RedirectToAction("Login", "Auth");

            conta.SaldoBRL += valor;

            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Deposito,
                Valor = valor,
                ContaOrigemId = conta.Id,
                DataHora = DateTime.Now
            });

            await _ctx.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // 💸 SACAR
        [HttpPost]
        public async Task<IActionResult> Sacar(decimal valor)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null) return RedirectToAction("Login", "Auth");

            if (conta.SaldoBRL >= valor + 2.5m)
            {
                conta.SaldoBRL -= (valor + 2.5m);

                _ctx.Transacoes.Add(new Transacao
                {
                    Tipo = TipoTransacao.Saque,
                    Valor = valor,
                    Taxa = 2.5m,
                    ContaOrigemId = conta.Id,
                    DataHora = DateTime.Now
                });

                await _ctx.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Cambio()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .Include(c => c.Cambios)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            return View(conta);
        }

        [HttpPost]
        public async Task<IActionResult> SimularCambio(decimal valor, string moeda)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .Include(c => c.Cambios)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            decimal taxa = 0;
            decimal convertido = 0;

            switch (moeda)
            {
                case "USD":
                    taxa = 5.0m;
                    convertido = valor / taxa;
                    break;

                case "EUR":
                    taxa = 5.4m;
                    convertido = valor / taxa;
                    break;

                case "GBP":
                    taxa = 6.2m;
                    convertido = valor / taxa;
                    break;

                case "JPY":
                    taxa = 0.035m;
                    convertido = valor / taxa;
                    break;
            }

            ViewBag.Simulacao = new
            {
                Valor = valor,
                Moeda = moeda,
                Taxa = taxa,
                Convertido = convertido
            };

            return View("Cambio", conta);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarCambio(decimal valor, string moeda)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            if (conta.SaldoBRL < valor)
                return RedirectToAction("Cambio");

            decimal convertido = 0;
            decimal taxa = 0;

            switch (moeda)
            {
                case "USD":
                    taxa = 5.0m;
                    convertido = Math.Round(valor / taxa, 2);
                    conta.SaldoUSD += convertido;
                    break;

                case "EUR":
                    taxa = 5.4m;
                    convertido = Math.Round(valor / taxa, 2);
                    conta.SaldoEUR += convertido;
                    break;

                case "GBP":
                    taxa = 6.2m;
                    convertido = Math.Round(valor / taxa, 2);
                    conta.SaldoGBP += convertido;
                    break;

                case "JPY":
                    taxa = 0.035m;
                    convertido = Math.Floor(valor / taxa); // sem decimal
                    conta.SaldoJPY += convertido;
                    break;
            }

            conta.SaldoBRL -= valor;

            var cambio = new Cambio
            {
                MoedaOrigem = "BRL",
                MoedaDestino = moeda,
                ValorOrigem = valor,
                ValorConvertido = convertido,
                TaxaCambio = taxa,
                ContaId = conta.Id,
                DataHora = DateTime.Now
            };

            _ctx.Cambios.Add(cambio);

            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Cambio,
                Valor = valor,
                ContaOrigemId = conta.Id,
                Descricao = $"Câmbio: R$ {valor:N2} → {convertido:N2} {moeda}",
                DataHora = DateTime.Now
            });

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var erro = ex.InnerException?.Message;
                throw new Exception(erro);
            }

            return RedirectToAction("Cambio");
        }

        public async Task<IActionResult> Cambios()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null) return RedirectToAction("Login", "Auth");

            var lista = await _ctx.Cambios
                .Where(c => c.ContaId == conta.Id)
                .OrderByDescending(c => c.DataHora)
                .ToListAsync();

            return View(lista);
        }

        // 🔁 TRANSFERIR
        [HttpPost]
        public async Task<IActionResult> Transferir(string destinoInput, decimal valor)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var origem = await _ctx.Contas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (origem == null)
                return RedirectToAction("Login", "Auth");

            // ✅ validações
            if (valor <= 0)
            {
                TempData["Erro"] = "Valor inválido";
                return RedirectToAction("Index");
            }

            if (origem.SaldoBRL < valor)
            {
                TempData["Erro"] = "Saldo insuficiente";
                return RedirectToAction("Index");
            }

            // 🔍 busca por email OU número da conta
            var destino = await _ctx.Contas
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.NumeroConta == destinoInput ||
                    c.Usuario.Email == destinoInput
                );

            if (destino == null)
            {
                TempData["Erro"] = "Conta ou email não encontrado";
                return RedirectToAction("Index");
            }

            if (destino.Id == origem.Id)
            {
                TempData["Erro"] = "Não pode transferir para si mesmo";
                return RedirectToAction("Index");
            }

            // 🔒 TRANSACTION (garante segurança)
            using var transaction = await _ctx.Database.BeginTransactionAsync();

            try
            {
                // 💸 TRANSFERÊNCIA
                origem.SaldoBRL -= valor;
                destino.SaldoBRL += valor;

                // 🔴 SAÍDA (quem enviou)
                _ctx.Transacoes.Add(new Transacao
                {
                    Tipo = TipoTransacao.TransferenciaEnviada,
                    Valor = valor,
                    ContaOrigemId = origem.Id,
                    ContaDestinoId = destino.Id,
                    Descricao = $"Transferência para {destino.Usuario.Email}",
                    DataHora = DateTime.Now
                });

                // 🟢 ENTRADA (quem recebeu)
                _ctx.Transacoes.Add(new Transacao
                {
                    Tipo = TipoTransacao.TransferenciaRecebida,
                    Valor = valor,
                    ContaOrigemId = origem.Id,
                    ContaDestinoId = destino.Id,
                    Descricao = $"Recebido de {origem.Usuario.Email}",
                    DataHora = DateTime.Now
                });

                await _ctx.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Sucesso"] = "Transferência realizada com sucesso";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Erro"] = "Erro ao realizar transferência";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Emprestimo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .Include(c => c.Emprestimos)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            return View(conta);
        }

        [HttpPost]
        public async Task<IActionResult> Emprestimo(decimal valor, int parcelas)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            if (valor > conta.LimiteEmprestimo)
            {
                TempData["Erro"] = $"Limite máximo: R$ {conta.LimiteEmprestimo:N2}";
                return RedirectToAction("Emprestimo");
            }

            var juros = 2.5m;
            var total = valor + (valor * juros / 100 * parcelas);
            var parcela = total / parcelas;

            ViewBag.Simulacao = new
            {
                Valor = valor,
                Parcelas = parcelas,
                Juros = juros,
                Parcela = parcela,
                Total = total
            };

            return View(conta);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarEmprestimo(decimal valor, int parcelas)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null)
                return RedirectToAction("Login", "Auth");

            if (valor <= 0)
            {
                TempData["Erro"] = "Valor inválido";
                return RedirectToAction("Emprestimo");
            }

            if (valor > conta.LimiteEmprestimo)
            {
                TempData["Erro"] = $"Limite máximo: R$ {conta.LimiteEmprestimo:N2}";
                return RedirectToAction("Emprestimo");
            }

            // 💳 cria empréstimo
            var emprestimo = new Emprestimo
            {
                ContaId = conta.Id,
                ValorSolicitado = valor,
                ValorAprovado = valor,
                NumeroParcelas = parcelas,
                TaxaJuros = 2.5m,
                Status = StatusEmprestimo.Aprovado,
                DataAprovacao = DateTime.Now
            };

            _ctx.Emprestimos.Add(emprestimo);

            // 💰 deposita na conta
            conta.SaldoBRL += valor;

            // 🧾 transação
            _ctx.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Emprestimo,
                Valor = valor,
                ContaOrigemId = conta.Id,
                Descricao = $"Empréstimo aprovado ({parcelas}x)",
                DataHora = DateTime.Now
            });

            await _ctx.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // 📄 EXTRATO
        public async Task<IActionResult> Extrato()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null) return RedirectToAction("Login", "Auth");

            var lista = await _ctx.Transacoes
                .Include(t => t.ContaOrigem).ThenInclude(c => c.Usuario)
                .Include(t => t.ContaDestino).ThenInclude(c => c.Usuario)
                .Where(t =>
                    (t.Tipo == TipoTransacao.TransferenciaEnviada && t.ContaOrigemId == conta.Id) ||
                    (t.Tipo == TipoTransacao.TransferenciaRecebida && t.ContaDestinoId == conta.Id) ||
                    (t.Tipo != TipoTransacao.TransferenciaEnviada && t.Tipo != TipoTransacao.TransferenciaRecebida && t.ContaOrigemId == conta.Id)
                )
                .OrderByDescending(t => t.DataHora)
                .ToListAsync();

            ViewBag.ContaId = conta.Id;
            return View(lista);
        }

        // 💳 CARTÕES
        public async Task<IActionResult> Cartoes()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var conta = await _ctx.Contas
                .Include(c => c.Cartoes)
                .ThenInclude(c => c.TipoCartao)
                .FirstOrDefaultAsync(c => c.UsuarioId == userId);

            if (conta == null) return RedirectToAction("Login", "Auth");

            return View(conta);
        }
    }
}