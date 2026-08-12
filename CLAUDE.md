# Nimbus Commerce Platform

## Purpose

Nimbus Commerce Platform is a cloud-native commerce platform built as a portfolio project to demonstrate modern enterprise software engineering.

## Goals

- Production-quality code
- Interview-ready architecture
- Incremental development
- AI-assisted engineering

## Tech Stack

### Backend
- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server

### Frontend
- React
- TypeScript
- Vite

### Infrastructure
- Docker
- Azure
- RabbitMQ (later)
- Redis (later)

## Architecture

- Modular Monolith
- Clean Architecture
- Feature-based organization inside the Application layer

## Current Projects

- NimbusCommerce.Api
- NimbusCommerce.Application
- NimbusCommerce.Domain
- NimbusCommerce.Infrastructure

## Layer Responsibilities

- **Api**
  - HTTP endpoints
  - Request/response contracts
  - Authentication/authorization configuration
  - OpenAPI/Scalar configuration
  - Maps HTTP contracts to/from Application models

- **Application**
  - Use cases
  - Business orchestration
  - Request models
  - Result models
  - Interfaces/abstractions
  - No infrastructure or presentation concerns

- **Domain**
  - Core business entities
  - Business rules
  - Domain logic only

- **Infrastructure**
  - Entity Framework Core
  - ASP.NET Core Identity
  - External services
  - Persistence
  - Third-party integrations

## Frontend Structure (`frontend/`)

React + TypeScript + Vite, React Router v7, CSS Modules. A separate `package.json`/build from the backend — not part of `NimbusCommerce.slnx`. See `docs/Architecture.md` → "Frontend" for the architectural reasoning (token storage, single-flight refresh, dev proxy) and `docs/development-setup.md` for running it.

- **`lib/api/`** — framework-agnostic HTTP transport (`apiFetch`, in-memory `tokenStore`, `ApiError`/`toApiError`). No React, no router imports — this is what lets the transport layer signal session expiry via a callback instead of an imperative navigation.
- **`features/<name>/`** — everything one feature owns, flat (mirrors the backend's "one folder per use case" convention in `Application/Authentication/*`): API calls, types, validation, components, and — for `auth` specifically — the auth state machine (`AuthContext`/`AuthProvider`/`useAuth`).
- **`components/`** — shared UI primitives used by ≥2 call sites (`Button`, `TextField`, `FormMessages`, `FullPageLoader`). Not a design system; don't add a component here for a single call site.
- **`layouts/`** — route-level chrome (`AuthLayout` for the centered login/register card, `AppLayout` for the authenticated application shell with nav).
- **`routes/`** — the route table (`router.tsx`) and its guards (`ProtectedRoute`, `PublicOnlyRoute`).
- **`styles/`** — `tokens.css` (CSS custom properties, light/dark) and `global.css` (reset + base typography) only.

Do not create a `features/products` or `features/orders` folder ahead of those features actually being built — `AppLayout`'s nav currently shows them as disabled placeholders on purpose (see `docs/engineering-handbook.md`).

## Engineering Philosophy

- Prefer simple, maintainable solutions over architectural purity.
- Introduce abstractions only when they solve a real problem.
- Avoid premature optimization and unnecessary layers.
- Prefer built-in .NET functionality before introducing third-party packages.
- Keep new implementations consistent with the existing architecture.

## Coding Principles

- Keep controllers thin.
- Business logic belongs in Application.
- Domain contains business rules only.
- Infrastructure contains external dependencies.
- Prefer async APIs.
- Use dependency injection.
- Follow SOLID principles.
- Prefer `internal sealed` for concrete service implementations.
- Follow existing project conventions before introducing new patterns.

## Documentation Workflow

Before implementing or refactoring code:

1. Determine which project documentation is relevant.
2. Read only the documentation needed for the current task.
3. Follow documented conventions instead of creating new ones.
4. If documentation conflicts with the requested implementation, explain the conflict before proceeding.

Do **not** read the entire `docs/` folder by default. Load only documentation relevant to the task.

## Documentation Map

Use the documentation as follows:

- **docs/Architecture.md**
  - Architecture decisions
  - Layer responsibilities
  - Project structure

- **docs/coding-standards.md**
  - Coding conventions
  - Naming
  - Design guidelines
  - Project-specific patterns

- **docs/project-journal.md**
  - Previous implementation decisions
  - Sprint history
  - Known technical debt
  - Deferred work

- Other documentation
  - Read only when directly related to the current task.

## AI Working Agreement

Before implementing anything:

1. Explain the proposed approach.
2. Identify which project documentation is relevant and consult it before implementation.
3. Follow the existing architecture and coding standards.
4. Don't introduce unnecessary abstractions.
5. Don't install packages unless requested or clearly justified.
6. Ask before making architectural changes.
7. If a proposed implementation conflicts with documented project conventions, stop and explain the tradeoff before proceeding.