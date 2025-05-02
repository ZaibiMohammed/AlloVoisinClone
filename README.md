# AlloVoisin Clone

A .NET 9 backend for a neighborhood sharing platform similar to AlloVoisin.

## Architecture

This project follows Clean Architecture principles with Domain-Driven Design:

### Core Layer
- **Domain**: Contains business entities, value objects, domain events, and business logic
- **Application**: Contains business use cases, commands/queries (CQRS), and interfaces for infrastructure

### Infrastructure Layer
- **Persistence**: Database access using Entity Framework Core 9
- **Identity**: Authentication and authorization using Identity Server
- **Infrastructure**: External services integration (payments, emails, etc.)

### Presentation Layer
- **API**: REST API controllers and GraphQL endpoints
- **SignalR**: Real-time communication hubs

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server or PostgreSQL
- Visual Studio 2025 or VS Code

### Running the Application

```bash
dotnet restore
dotnet build
dotnet run --project src/Presentation/API/AlloVoisinClone.API
```

## Technologies

- .NET 9
- Entity Framework Core 9
- MediatR (for CQRS)
- Identity Server
- SignalR
- FluentValidation
- AutoMapper
- Hangfire (background jobs)
