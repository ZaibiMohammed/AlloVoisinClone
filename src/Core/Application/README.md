# Application Layer

This layer contains all application logic. It is dependent on the domain layer, but has no dependencies on any other layer or project. This layer defines interfaces that are implemented by outside layers.

## Folders

- **Common**: Contains common classes used by the application layer
- **Features**: Contains feature folders organized by domain entity
  - **/User**: User-related features
  - **/Item**: Item-related features
  - **/Rental**: Rental-related features
  - **/Payment**: Payment-related features
  - **/Review**: Review-related features
- **Behaviors**: Contains MediatR pipeline behaviors for cross-cutting concerns
- **Interfaces**: Contains interfaces for infrastructure services
- **Mappings**: Contains AutoMapper mapping profiles
- **Models**: Contains DTOs (Data Transfer Objects) and ViewModels

## CQRS Pattern

Each feature folder follows the CQRS pattern:

- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, List, Search)
- **Events**: Domain events triggered by commands

## Command and Query Examples

```csharp
// Command Example
public record CreateUserCommand : IRequest<UserDto>
{
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    // Other properties
}

// Query Example
public record GetUserByIdQuery : IRequest<UserDto>
{
    public Guid Id { get; init; }
}
```
