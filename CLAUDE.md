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