BankMore API - Manual de Execução
=================================

API de gerenciamento de contas correntes da fintech BankMore.

1. URLs de Acesso
-----------------
- HTTP: http://localhost:5122
- HTTPS: https://localhost:7256
- Swagger: 
  - https://localhost:7256/swagger/index.html
  - http://localhost:5122/swagger/index.html

2. Executando o Projeto
----------------------
1. Abra o terminal ou Visual Studio Code/Visual Studio.
2. Navegue até a pasta do projeto.
3. Execute:
   dotnet run
4. Verifique no terminal as URLs ativas (HTTP/HTTPS conforme launchSettings.json).

3. Endpoints da API
-------------------

3.1 Criar Conta
POST /api/contacorrente/criar
URL HTTPS: https://localhost:7256/api/contacorrente/criar
Body JSON:
{
  "CPF": "12345678901",
  "Nome": "Ana Silva",
  "Senha": "senhaSegura123"
}
Resposta 200 OK:
{
  "numero": 123456,
  "id": "guid-da-conta"
}
Erros:
- 400 BadRequest CPF inválido:
{
  "message": "CPF invalido",
  "type": "INVALID_DOCUMENT"
}

3.2 Login
POST /api/contacorrente/login
Body JSON:
{
  "CPFOuNumero": "12345678901",
  "Senha": "senhaSegura123"
}
Resposta 200 OK:
{
  "token": "jwt-token"
}
Erros:
- 401 Unauthorized:
{
  "message": "Usuario nao autorizado",
  "type": "USER_UNAUTHORIZED"
}

3.3 Inativar Conta
POST /api/contacorrente/inativar
Headers:
Authorization: Bearer <jwt-token>
Body JSON:
{
  "senha": "senhaSegura123"
}
Resposta: 204 No Content
Erros:
- 403 Forbidden se JWT inválido ou usuário não autorizado.

3.4 Movimentação
POST /api/contacorrente/movimentacao
Headers:
Authorization: Bearer <jwt-token>
Body JSON:
{
  "IdRequisicao": "req-123",
  "NumeroConta": 123456,
  "Valor": 100.50,
  "Tipo": "C"
}
Tipo de movimentação:
- "C" → Crédito
- "D" → Débito
Resposta: 204 No Content
Erros:
- 400 BadRequest:
  {
    "message": "Valor deve ser positivo",
    "type": "INVALID_VALUE"
  }
  {
    "message": "Tipo invalido",
    "type": "INVALID_TYPE"
  }

3.5 Saldo
GET /api/contacorrente/saldo
Headers:
Authorization: Bearer <jwt-token>
Resposta 200 OK:
{
  "numero": 123456,
  "nome": "Ana Silva",
  "data": "20/09/2025 10:00:00",
  "saldo": "0,00"
}

3.6 Transferência
POST /api/contacorrente/transferencia
Headers:
Authorization: Bearer <jwt-token>
Body JSON:
{
  "IdRequisicao": "req-456",
  "NumeroContaDestino": 654321,
  "Valor": 50.00,
  "ContaOrigemId": "guid-conta-origem"
}
Resposta: 204 No Content
Erros:
- 400 BadRequest:
  {
    "message": "Conta destino invalida",
    "type": "INVALID_ACCOUNT"
  }
  {
    "message": "Valor invalido",
    "type": "INVALID_VALUE"
  }

4. Testando via Swagger
----------------------
1. Abra o Swagger no navegador:
   https://localhost:7256/swagger/index.html
2. Clique em cada endpoint para expandir.
3. Use "Try it out" para testar as chamadas.
4. Para endpoints que exigem JWT (/saldo, /movimentacao, /transferencia, /inativar),
   copie o token retornado pelo login e cole no campo "Authorize".

5. Testando via Postman
----------------------
1. Crie uma nova Collection no Postman.
2. Adicione requisições com os métodos e URLs conforme acima.
3. Para endpoints protegidos, adicione Header:
   Authorization: Bearer <jwt-token>
4. Use JSON nos corpos de requisição conforme os exemplos acima.
