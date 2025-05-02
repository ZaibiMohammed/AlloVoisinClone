# Persistence Infrastructure Layer

This layer contains classes for accessing external resources such as databases, file systems, web services, SMTP, etc. These classes implement interfaces defined in the Application layer.

## Folders

- **Context**: Contains the EF Core DbContext and related configurations
- **Repositories**: Contains repository implementations for each entity
- **Migrations**: Contains database migrations
- **Seed**: Contains database seed data
- **Configurations**: Contains entity type configurations for EF Core

## Database Context

The `ApplicationDbContext` is the main DbContext for the application. It's configured to use SQL Server by default, but can be configured to use PostgreSQL or other databases as well.

## Entity Configurations

Entity configurations are implemented using the Fluent API:

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();
            
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();
            
        // ... other configurations
    }
}
```

## Repository Pattern

Repositories provide a consistent abstraction layer over data access:

```csharp
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    // ... other methods
}
```
