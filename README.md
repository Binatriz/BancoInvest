BancoInvest

Sistema bancário digital desenvolvido em ASP.NET Core MVC + Web API, utilizando:
ASP.NET Core MVC
ASP.NET Core Web API
Entity Framework Core
SQL Server
Identity Framework
JWT Authentication
Swagger
Bootstrap 5
Session
DTOs
CRUD completo

📌 Sobre o Projeto
O BancoInvest é uma aplicação bancária digital que permite:
1. Cadastro e login de usuários
2. Autenticação JWT
3. Consulta de conta
4. Depósitos
5. Saques
6. Transferências
7. Câmbio de moedas
8. Empréstimos
9. Cartões de crédito/débito
10. Extrato bancário
11. Swagger para testes da API

Tecnologias Utilizadas:
- C#
- ASP.NET Core 8
- Entity Framework Core
- SQL Server
- Identity Framework
- JWT Bearer
- Swagger / Swashbuckle
- Bootstrap 5
- Razor Views
  
🔐 Autenticação JWT
A API utiliza JWT Bearer Token.
Após login:
A API retorna um token
O token deve ser enviado no Swagger
Todas as rotas protegidas utilizam:
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

⚙️ Como Rodar o Projeto
1. Executar projeto
🌐 Swagger
- Swagger ficará disponível em:
https://localhost:7153/swagger

🔑 Como Autenticar no Swagger
1. Fazer Login
Endpoint:
POST /api/AuthApi/login
- Body:
{
  "email": "teste1@teste.com",
  "password": "Admin@123"
}

- Resposta:
{
  "token": "SEU_TOKEN_AQUI"
}

2. Copiar apenas o token
- COPIE SOMENTE:
eyJhbGciOi...
- NÃO copie:
{
  "token": ""
}

3. Clicar em "Authorize" no Swagger
Botão no topo direito.


👤 Usuários para Teste
- Usuário 1:
Email: teste1@teste.com
Senha: Admin@123

- Usuário 2:
Email: teste2@teste.com
Senha: Admin@123

🧪 Endpoints da API
🔐 AUTH
Login
POST /api/AuthApi/login

- Body:
{
  "email": "teste1@teste.com",
  "password": "Admin@123"
}

🏦 CONTA
Obter Conta
GET /api/ContaApi
Retorna:
- saldo
- moedas
- cartões
- empréstimos
- câmbios
- usuário

Debug Usuário
- GET /api/ContaApi/debug-user
- Retorna:
{
  "userId": "...",
  "userName": "...",
  "email": "..."
}


💰 DEPÓSITO
POST /api/ContaApi/depositar

Body:
{
  "valor": 500
}

🏧 SAQUE
POST /api/ContaApi/sacar

Body:
{
  "valor": 100
}
Taxa:
R$ 2,50

🔁 TRANSFERÊNCIA
Pode transferir usando:
- email
- número da conta
POST /api/ContaApi/transferir

Body:
{
  "destinoInput": "teste2@teste.com",
  "valor": 50
}


📄 EXTRATO
GET /api/ContaApi/extrato
Retorna:
- depósitos
- saques
- transferências
- empréstimos
- câmbios

💱 CÂMBIO
Simulação
POST /api/ContaApi/cambio

Body:
{
  "valor": 1000,
  "moeda": "USD"
}

Moedas:
- USD
- EUR
- GBP
- JPY

💳 CARTÕES
- Listar cartões
Os cartões já retornam em:
GET /api/ContaApi
Adicionar cartão
POST /api/ContaApi/cartao

Body:
{
  "tipoCartaoId": 1,
  "bandeira": "Visa"
}

Tipos:
1 = Crédito
2 = Débito


Excluir cartão:
- DELETE /api/ContaApi/cartao/{id}
Exemplo:
- DELETE /api/ContaApi/cartao/1

💳 Empréstimos
Simular empréstimo
POST /api/ContaApi/emprestimo

Body:
{
  "valor": 5000,
  "parcelas": 12
}

🧠 Regras de Negócio

1. Saque
- taxa fixa de R$ 2,50

2. Transferência = Não permite:
- saldo insuficiente
- transferência para si mesmo

3. Cartões (Só pode ter no máximo 1 cartão de crédito e 1 cartão de débito)
  
Cartão Crédito:
- possui limite

Cartão Débito:
- não possui limite
