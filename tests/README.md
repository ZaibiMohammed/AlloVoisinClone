# Tests

This folder contains all tests for the AlloVoisin Clone application. The tests are organized by type and layer.

## Folder Structure

- **UnitTests**: Contains unit tests for all layers
  - **Core**: Tests for the Core layer
    - **Domain**: Tests for domain entities, value objects, and domain services
    - **Application**: Tests for application services, commands, and queries
  - **Infrastructure**: Tests for infrastructure services
    - **Persistence**: Tests for repositories and the database context
    - **Identity**: Tests for identity services
    - **Infrastructure**: Tests for external services
  - **Presentation**: Tests for the presentation layer
    - **API**: Tests for API controllers
    - **SignalR**: Tests for SignalR hubs

- **IntegrationTests**: Contains integration tests
  - **API**: Tests for API controllers with real dependencies
  - **Infrastructure**: Tests for infrastructure services with real dependencies

- **FunctionalTests**: Contains end-to-end tests
  - **API**: Tests for API endpoints from HTTP requests to database changes

- **Common**: Contains common test utilities and fixtures

## Testing Frameworks

- **xUnit**: The main testing framework
- **Moq**: For mocking dependencies
- **FluentAssertions**: For more readable assertions
- **Bogus**: For generating test data
- **Respawn**: For cleaning the database between tests

## Example Unit Test

```csharp
public class CreateUserCommandHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    
    public CreateUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _emailServiceMock = new Mock<IEmailService>();
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "P@ssw0rd"
        };
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName
        };
        
        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(user);
        
        var handler = new CreateUserCommandHandler(
            _mapper,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _emailServiceMock.Object);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(
            command.Email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()),
            Times.Once);
    }
}
```

## Example Integration Test

```csharp
public class UserControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public UserControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the database context with an in-memory database
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
        
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetUsers_ReturnsSuccessAndUsers()
    {
        // Arrange - Seed the database
        // ...
        
        // Act
        var response = await _client.GetAsync("/api/v1/users");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<PaginatedList<UserDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        users.Should().NotBeNull();
        users.Items.Should().NotBeEmpty();
    }
}
```
