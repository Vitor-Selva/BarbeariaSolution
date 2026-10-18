# Contexto do Projeto: API REST para Barbearia

Este documento serve como base de contexto para o desenvolvimento de uma API RESTful em .NET 10 destinada ao gerenciamento de uma barbearia. A API gerencia serviços, agendamentos e autenticação com diferentes níveis de acesso (Admin e Cliente), contando com deploy automatizado.

## ⏱️ Estratégia de Desenvolvimento e Ambiente

* **IDE Padrão:** Visual Studio Code (VS Code).

* **Restrição de Tempo:** Há pouco tempo disponível para a conclusão do projeto, com o risco de não dar tempo de terminar todos os requisitos secundários.

* **Priorização (Foco no Essencial):** A abordagem de desenvolvimento deve focar em entregar o **mais importante primeiro**. A ordem de prioridade será:

  1. Configuração inicial, banco de dados e deploy automático (esqueleto no ar).

  2. Domínio e CRUD da entidade principal (`Servico`).

  3. Autenticação e Autorização (registro, login, JWT).

  4. Segunda entidade (`Agendamento`) e a **regra de negócio de conflito (HTTP 409)**.

  5. Testes unitários essenciais e refinamentos (apenas se sobrar tempo).

## 🛠️ Tecnologias Utilizadas

* **Plataforma:** .NET 10 (ASP.NET Core Web API, C# 14).

* **Banco de Dados:** PostgreSQL (hospedado na Neon).

* **ORM:** Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.Design`, `Npgsql.EntityFrameworkCore.PostgreSQL`).

* **Autenticação e Segurança:** JWT (JSON Web Token) via `JwtBearer` e criptografia de senhas com `BCrypt.Net-Next`.

* **Documentação:** Swagger (`Swashbuckle.AspNetCore`). *Nota: Não usar o OpenAPI nativo do .NET 10.*

* **Testes Automatizados:** xUnit com banco de dados em memória SQLite (`Microsoft.EntityFrameworkCore.Sqlite`).

* **DevOps e Infraestrutura:** Docker (conteinerização em 2 estágios na porta 10000), GitHub Actions (CI para build e testes), Render (Deploy automático).

## 🗄️ Estrutura do Banco de Dados

O banco de dados PostgreSQL deve conter as seguintes entidades principais (com datas sempre em **UTC**):

1. **Usuario:**

   * Representa administradores e clientes.

   * Campos base: ID, Nome, Email, Senha (hash gerado com BCrypt), Role (`admin` ou `cliente`).

2. **Servico (Entidade Principal):**

   * Representa os serviços oferecidos.

   * Campos base: ID, Nome (ex: Corte Degradê, Barba), Descrição, Preço, Duração (em minutos).

3. **Agendamento (Segunda Entidade):**

   * Representa a marcação de um serviço (Relação 1:N com Serviço e 1:N com Usuário).

   * Campos base: ID, DataHora (UTC), ServicoId, UsuarioId.

**🚨 Regra de Negócio Crítica (HTTP 409 - Conflict):**
O sistema **NÃO** deve permitir a criação de dois agendamentos no mesmo horário. A validação deve ocorrer no serviço (dentro de uma transação) e retornar o status HTTP `409 Conflict` (via `ProblemDetails`) caso haja sobreposição de horários.

## ⚙️ Estrutura e Padrões da API (Backend)

* **Arquitetura (Camadas):**

  * **Controllers Magros:** Apenas recebem requisições, delegam para serviços e retornam HTTP Status Codes baseados no resultado.

  * **Serviços:** Contêm a regra de negócio (ex: validação do 409), interações com EF Core e retornam objetos padronizados (ex: `Resultado<T>`).

  * **DTOs:** Utilizados rigorosamente para entrada (Request) e saída (Response). Entidades do banco nunca são expostas.

  * DAOs: Acessam o banco de dados.

  * MVC: Usar uma estrutura de model-view-controller.

* **Erros e Validações:**

  * Uso global de `ProblemDetails`.

  * Validações de entrada (ex: DataAnnotations nos DTOs) retornam `400 Bad Request`.

  * Implementação de logs estruturados (`ILogger`) para eventos de negócio.

* **Autenticação e Autorização:**

  * Operações de escrita de `Servico` (POST, PUT, DELETE) exclusivas para a role `admin`.

  * Clientes operam apenas seus próprios `Agendamentos`. Tentar acessar/alterar dados de terceiros retorna `403 Forbidden`. Acesso sem token retorna `401 Unauthorized`.

* **Endpoints Especiais e Comportamentos:**

  * **Listagem Pública:** GET de Serviços deve ser paginado (`?pagina=&tamanho=`) e possuir filtro de busca (`?busca=`).

  * **Saúde e CI:** Endpoints `/health` (testando banco) e `/api/versao` (retornando hash do commit da variável de ambiente).

  * **Seed:** `Program.cs` deve rodar `db.Database.MigrateAsync()` e um Seed idempotente populando 1 admin (senha via env var), 2 clientes e no mínimo 8 serviços.

  * **Segurança de Configuração:** Nenhuma *connection string*, chave JWT ou senha hardcoded ou no appsettings. Uso estrito de Variáveis de Ambiente.