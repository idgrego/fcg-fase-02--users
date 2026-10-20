# UsersAPI - Documentação de Arquitetura

O **UsersAPI** é o microsserviço responsável pela gestão de identidade, controle de acesso e ciclo de vida de usuários da plataforma **FIAP Cloud Games**.

---

## 📑 Sumário

- [1. Visão Geral e Responsabilidades](#1-visão-geral-e-responsabilidades)
- [2. Padrão Arquitetural: Clean Architecture (.NET 10+)](#2-padrão-arquitetural-clean-architecture-net-10)
  - [2.1. Regra de Dependência](#21-regra-de-dependência)
  - [2.2. Decisão Arquitetural: Minimal APIs vs Controllers](#22-decisão-arquitetural-minimal-apis-vs-controllers)
- [3. Banco de Dados: PostgreSQL](#3-banco-de-dados-postgresql)
  - [3.1. Esquema Conceitual de Dados](#31-esquema-conceitual-de-dados)
- [4. Integração de Eventos (RabbitMQ)](#4-integração-de-eventos-rabbitmq)

---

## 1. Visão Geral e Responsabilidades

- **Cadastro de Usuários:** Criação de conta com hash seguro de senha e validação de unicidade de e-mail.
- **Autenticação & JWT:** Emissão e validação de tokens JWT para sessões de usuários.
- **Perfis de Acesso (Roles):**
  - `anonymous`: Usuário não autenticado ou com acesso restrito a navegação.
  - `gamer`: Usuário jogador (compra de jogos, visualização de biblioteca).
  - `admin`: Usuário administrador (gestão de catálogo e usuários).
- **Publicação de Eventos:** Emissão do evento `UserCreatedEvent` via RabbitMQ após o cadastro de um novo usuário.

---

## 2. Padrão Arquitetural: Clean Architecture (.NET 10+)

A solução é organizada segundo a **Clean Architecture** (Arquitetura Limpa), adotando **.NET 10 (`net10.0`)**, formato de solução `.slnx` e **Minimal APIs** para a camada de apresentação. Isso garante máxima performance, menor footprint de memória e total isolamento das regras de negócio (Domínio):

```text
fcg-fase-02--users/
├── FCG02.UsersAPI.slnx           # Solução .NET 10 no formato moderno slnx
├── docs/
│   └── architecture.md           # Documentação arquitetural do serviço
├── src/
│   ├── FCG02.UsersAPI.Domain/          # Núcleo: Entidades, Value Objects, Enums e Interfaces de Repositório
│   ├── FCG02.UsersAPI.Application/     # Casos de uso: DTOs, Services/Use Cases, Interfaces (Token, Eventos)
│   ├── FCG02.UsersAPI.Infrastructure/  # PostgreSQL (EF Core), BCrypt, RabbitMQ Publisher
│   └── FCG02.UsersAPI.API/             # Minimal APIs (Route Groups), Middlewares, Scalar (OpenAPI) e Program.cs
└── tests/
    └── FCG02.UsersAPI.UnitTests/       # Testes unitários de Domínio e Casos de Uso
```

### 2.1. Regra de Dependência

```mermaid
graph TD
    subgraph Apresentacao ["Camada de Apresentação"]
        API["FCG02.UsersAPI.API<br/>(Minimal APIs / Route Groups / Middlewares / Scalar)"]
    end

    subgraph CasosDeUso ["Camada de Aplicação"]
        App["FCG02.UsersAPI.Application<br/>(Use Cases / DTOs / Interfaces)"]
    end

    subgraph Infra ["Camada de Infraestrutura"]
        InfraProj["FCG02.UsersAPI.Infrastructure<br/>(EF Core / PostgreSQL / RabbitMQ / BCrypt)"]
    end

    subgraph Dominio ["Núcleo do Negócio"]
        Domain["FCG02.UsersAPI.Domain<br/>(Entidades / Enums / Regras / Interfaces)"]
    end

    API --> App
    API --> InfraProj
    InfraProj --> App
    InfraProj --> Domain
    App --> Domain
```
- **Domain:** Não referencia nenhum outro projeto. Não referencia EF Core nem bibliotecas externas.
- **Application:** Referencia apenas o **Domain**.
- **Infrastructure:** Referencia **Application** e **Domain**.
- **API:** Referencia **Application** e **Infrastructure** (para configuração do container de DI no `Program.cs`).

### 2.2. Decisão Arquitetural: Minimal APIs vs Controllers
- **Padrão Escolhido:** **Minimal APIs** modulares por contexto de negócio (ex.: `AuthEndpoints.cs`, `UserEndpoints.cs`).
- **Justificativas Técnicas:**
  1. **Performance e Cold-Start:** Inicialização substancialmente mais rápida que Controllers convencionais, eliminando camadas desnecessárias da pipeline do MVC (filtros complexos, action invoker, reflection pesada).
  2. **Adequação a Serverless / Cloud-Native:** Ideal para escalonamento elástico no Azure (Container Apps com scale-to-zero ou Azure Functions), reduzindo latência da primeira requisição.
  3. **Preparação para Native AOT:** No .NET 10, Minimal APIs possuem suporte nativo de primeira classe para compilação antecipada (Native AOT), permitindo contêineres minúsculos (<30MB) e consumo de RAM na faixa de 15MB a 30MB.
  4. **Modularização Limpa:** A organização das rotas através de métodos de extensão (`app.MapGroup("/api/v1/auth")`) garante código coeso e desacoplado, sem sobrecarregar o `Program.cs`.
- **Domain:** Não referencia nenhum outro projeto. Não referencia EF Core nem bibliotecas externas.
- **Application:** Referencia apenas o **Domain**.
- **Infrastructure:** Referencia **Application** e **Domain**.
- **API:** Referencia **Application** e **Infrastructure** (para configuração do container de DI no `Program.cs`).

---

## 3. Banco de Dados: PostgreSQL

- **ORM:** Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- **Mapeamento:** Fluent API (`IEntityTypeConfiguration<T>`) na camada de `Infrastructure`, mantendo o `Domain` livre de anotações de dados de banco.
- **Migrações:** Gerenciadas via EF Core Migrations.

### 3.1. Esquema Conceitual de Dados

Tabela: `Users`
- `Id` (UUID/Guid, PK)
- `Name` (VARCHAR(150), NOT NULL)
- `Email` (VARCHAR(200), NOT NULL, UNIQUE INDEX)
- `PasswordHash` (VARCHAR(255), NOT NULL)
- `Role` (VARCHAR(20) ou INT, NOT NULL - `anonymous`, `gamer`, `admin`)
- `CreatedAt` (TIMESTAMPTZ, NOT NULL)
- `UpdatedAt` (TIMESTAMPTZ, NULL)

---

## 4. Integração de Eventos (RabbitMQ)

Ao persistir um novo usuário com sucesso no PostgreSQL:
1. A camada de aplicação invoca o `IEventPublisher`.
2. O evento `UserCreatedEvent` é publicado na exchange do RabbitMQ (`users.events` ou `amq.topic`).
3. Payload do evento:
   ```json
   {
     "eventId": "UUID",
     "occurredOn": "2026-10-03T10:00:00Z",
     "userId": "UUID",
     "name": "Nome do Usuário",
     "email": "usuario@exemplo.com",
     "role": "gamer"
   }
   ```
