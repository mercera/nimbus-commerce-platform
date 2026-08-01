# Nimbus Commerce Platform

## Purpose

Nimbus Commerce Platform is a cloud-native commerce platform built as a portfolio project to demonstrate modern enterprise software engineering.

## Goals

- Production-quality code
- Interview-ready architecture
- Incremental development
- AI-assisted engineering

## Tech Stack

Backend
- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server

Frontend
- React
- TypeScript
- Vite

Infrastructure
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

## Coding Principles

- Keep controllers thin.
- Business logic belongs in Application.
- Domain contains business rules only.
- Infrastructure contains external dependencies.
- Prefer async APIs.
- Use dependency injection.
- Follow SOLID principles.

## AI Rules

Before implementing anything:

1. Explain the approach.
2. Follow the existing architecture.
3. Don't introduce unnecessary abstractions.
4. Don't install packages unless requested.
5. Ask before making architectural changes.