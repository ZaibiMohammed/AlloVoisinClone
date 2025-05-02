# SignalR Hubs Layer

This layer contains SignalR hubs for real-time communication between clients and the server. It's primarily used for chat, notifications, and real-time updates.

## Folders

- **Hubs**: Contains SignalR hub classes
- **Clients**: Contains interfaces for strongly-typed clients
- **Filters**: Contains hub filters for cross-cutting concerns
- **Extensions**: Contains extension methods for configuring SignalR

## Chat Hub Example

```csharp
public interface IChatClient
{
    Task ReceiveMessage(ChatMessageDto message);
    Task UserJoined(string username);
    Task UserLeft(string username);
    Task TypingStarted(string username);
    Task TypingStopped(string username);
}

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;
    
    public ChatHub(IMediator mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    public override async Task OnConnectedAsync()
    {
        var username = Context.User.Identity.Name;
        var conversationId = Context.GetHttpContext().Request.Query["conversationId"].ToString();
        
        if (!string.IsNullOrEmpty(conversationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            await Clients.Group(conversationId).UserJoined(username);
            
            _logger.LogInformation("User {Username} joined conversation {ConversationId}", username, conversationId);
        }
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User.Identity.Name;
        var conversationId = Context.GetHttpContext().Request.Query["conversationId"].ToString();
        
        if (!string.IsNullOrEmpty(conversationId))
        {
            await Clients.Group(conversationId).UserLeft(username);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
            
            _logger.LogInformation("User {Username} left conversation {ConversationId}", username, conversationId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(SendMessageCommand command)
    {
        var message = await _mediator.Send(command);
        
        await Clients.Group(command.ConversationId.ToString()).ReceiveMessage(message);
    }
    
    public async Task StartTyping(string conversationId)
    {
        var username = Context.User.Identity.Name;
        
        await Clients.OthersInGroup(conversationId).TypingStarted(username);
    }
    
    public async Task StopTyping(string conversationId)
    {
        var username = Context.User.Identity.Name;
        
        await Clients.OthersInGroup(conversationId).TypingStopped(username);
    }
}
```

## Notification Hub Example

```csharp
public interface INotificationClient
{
    Task ReceiveNotification(NotificationDto notification);
}

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private readonly IUserConnectionManager _userConnectionManager;
    
    public NotificationHub(IUserConnectionManager userConnectionManager)
    {
        _userConnectionManager = userConnectionManager;
    }
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        _userConnectionManager.AddConnection(userId, Context.ConnectionId);
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        _userConnectionManager.RemoveConnection(userId, Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }
}
```

## SignalR Configuration

```csharp
public static class SignalRExtensions
{
    public static IServiceCollection AddSignalRConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 102400; // 100 KB
            options.StreamBufferCapacity = 10;
        })
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        })
        .AddMessagePackProtocol();
        
        // Add Redis backplane for distributed environments
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "AlloVoisinClone:";
            });
            
            services.AddSignalR().AddStackExchangeRedis(redisConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = "AlloVoisinClone";
            });
        }
        
        // Add user connection manager
        services.AddSingleton<IUserConnectionManager, UserConnectionManager>();
        
        return services;
    }
    
    public static IApplicationBuilder UseSignalRConfig(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/hubs/chat");
            endpoints.MapHub<NotificationHub>("/hubs/notification");
        });
        
        return app;
    }
}
```
