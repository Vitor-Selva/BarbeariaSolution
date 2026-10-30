# BarbeariaAPI

API REST para gerenciar uma barbearia: catálogo de serviços, agendamentos e autenticação com JWT.

> **Aviso:** este é um **projeto escolar**, desenvolvido apenas para fins de estudo (curso de Tecnologia em Análise e Desenvolvimento de Sistemas). Não utilize dados reais nem este sistema em produção.

## Links

| O quê | Endereço |
|---|---|
| API em produção | https://barbearia-api-glxg.onrender.com |
| Swagger (documentação interativa) | https://barbearia-api-glxg.onrender.com/swagger |
| Versão em produção (SHA do commit) | https://barbearia-api-glxg.onrender.com/api/versao |
| Repositório | https://github.com/Vitor-Selva/BarbeariaSolution |

> A API roda no plano gratuito do Render: após um tempo sem uso ela "dorme", e a **primeira requisição pode levar cerca de 1 minuto**. As seguintes respondem normalmente.

## Tecnologias

- .NET 10 e ASP.NET Core (Web API com Controllers)
- Entity Framework Core 10 com Npgsql e PostgreSQL (Neon)
- Autenticação JWT (HS256) com senhas protegidas por BCrypt
- Swagger (Swashbuckle) com botão Authorize
- Docker, GitHub Actions (CI) e deploy automático no Render

## Arquitetura

O código fica em `BarbeariaAPI/`, organizado em camadas:

`Controllers → Services → DAOs → Banco`, com `DTOs` para entrada e saída, `Entities` para o modelo e `Exceptions` para os erros de negócio. Um `ApiExceptionHandler` converte as exceções em respostas HTTP (400, 404 e 409, no formato ProblemDetails).

## Rotas (para o corretor)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/` | Público | Mensagem de status |
| GET | `/health` | Público | Verificação de saúde |
| GET | `/api/versao` | Público | JSON com o SHA do commit em produção |
| POST | `/api/auth/registro` | Público | Cria uma conta de cliente (201) |
| POST | `/api/auth/login` | Público | Retorna o token JWT (200) |
| GET | `/api/servicos` | Público | Lista paginada de serviços |
| GET | `/api/servicos/{id}` | Público | Detalhe de um serviço |
| POST | `/api/servicos` | admin | Cria serviço (201) |
| PUT | `/api/servicos/{id}` | admin | Atualiza serviço |
| DELETE | `/api/servicos/{id}` | admin | Remove serviço (204; 409 se houver agendamentos) |
| GET | `/api/agendamentos` | Logado | Lista paginada (cliente vê só os seus; admin vê todos) |
| GET | `/api/agendamentos/{id}` | Logado | Detalhe de um agendamento |
| POST | `/api/agendamentos` | Logado | Cria agendamento (409 se o horário conflitar) |
| PUT | `/api/agendamentos/{id}` | Logado | Reagenda |
| DELETE | `/api/agendamentos/{id}` | Logado | Cancela (204) |

**Paginação:** `GET /api/servicos` aceita `busca` (opcional, por nome ou descrição), `pagina` (padrão 1) e `tamanho` (padrão 10, máximo 50). Resposta:

```json
{ "itens": [], "pagina": 1, "tamanho": 10, "total": 0 }
```

A listagem de agendamentos também é paginada e aceita `data` e, para admin, `usuarioId`.

**Datas:** `dataHora` deve ser futura e enviada em UTC, por exemplo `2026-11-03T14:00:00Z`.

**Códigos de erro:** 400 (validação), 401 (sem token ou credenciais inválidas), 403 (papel sem permissão), 404 (não encontrado) e 409 (conflito de horário, e-mail já cadastrado ou serviço com agendamentos).

## Contas de teste

Como o repositório está público, optei por adicionar elas no arquivo de envio.

**Como testar no Swagger:** faça login em `POST /api/auth/login` com e-mail e senha, copie o `token` da resposta, clique em **Authorize** e cole só o token.

## Como rodar localmente

Requisitos: .NET 10 SDK e um PostgreSQL (o projeto usa o Neon).

```bash
cd BarbeariaAPI
dotnet user-secrets set "Jwt:Key" "<chave com pelo menos 32 caracteres>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string do PostgreSQL>"
dotnet ef database update
dotnet run
```

A API sobe em `http://localhost:5260` e o Swagger fica em `/swagger`. Em produção, as mesmas configurações vêm das variáveis de ambiente `Jwt__Key` e `ConnectionStrings__DefaultConnection`. A API **não inicia** se a `Jwt:Key` faltar ou tiver menos de 32 caracteres.

Com Docker, crie um `.env` (não versionado) com essas duas variáveis e rode `docker compose up --build`; a API fica na porta 10000.

## Deploy e CI

- A cada push no `main`, o **GitHub Actions** compila o projeto (`.github/workflows/ci.yml`) e o **Render** publica a nova versão automaticamente, usando o `Dockerfile`.
- O `/api/versao` devolve o SHA completo do commit em execução, lido da variável `RENDER_GIT_COMMIT` do Render. Localmente retorna `"desconhecido"`.

## Front-end

Não foi desenvolvido nesta entrega: o projeto contém apenas a API. Toda a interação é feita pelo Swagger ou por um cliente HTTP (Postman).

## Registro do uso de IA

**Qual IA:** Claude (Anthropic), na versão gratuita pela web, alternando entre os modelos Sonnet 5.5 e Haiku 5.5.

**Três pedidos feitos à IA:**
1. Implementar a autenticação: registro, login com BCrypt, token JWT e os papéis `admin` e `cliente` com `[Authorize(Roles = ...)]`.
2. Colocar a API em produção com deploy automático no Render, CI no GitHub Actions e o endpoint `/api/versao` com o SHA do commit.
3. Documentar a API no Swagger: descrição com aviso de projeto escolar, botão Authorize (Bearer) e códigos de resposta nas rotas de escrita.

**Erro da IA:** pelo que percebi, a IA não cometeu nenhum erro neste projeto. As dificuldades que surgiram vieram da minha falta de conhecimento ou de eu não seguir exatamente o que foi pedido, ou seja, foram erros humanos. Dois exemplos:
- No primeiro deploy, o build do Docker falhou (`BarbeariaAPI.csproj: not found`) porque eu não configurei o *Docker Build Context Directory* do Render para a pasta `BarbeariaAPI`. Corrigi o campo no painel e o deploy funcionou.
- Um `git pull` deixou um merge incompleto (`main|MERGING`) com conflito no `Program.cs`; resolvi o conflito, conferi que não restaram marcadores e concluí o merge com um commit.

**O que aprendi:**
- O contexto de build do Docker define quais arquivos o `COPY` enxerga, e isso importa em repositórios com várias pastas.
- O `__` nas variáveis de ambiente equivale ao `:` da configuração do .NET, e segredos ficam fora do repositório.
- O SHA do commit em produção prova qual versão está rodando, e o CI verde prova que esse mesmo commit compila.
- Como funciona um merge no Git e como resolver um conflito.