# Infrastructure Services Layer

This layer contains services that implement interfaces defined in the Application layer. These services interact with external systems such as payment gateways, email services, file storage, etc.

## Folders

- **Services**: Contains service implementations
  - **Payment**: Payment gateway integrations (Stripe, PayPal)
  - **Email**: Email service implementations
  - **SMS**: SMS service implementations
  - **Storage**: File storage implementations (Azure Blob, AWS S3)
  - **Geocoding**: Geocoding service implementations (Google Maps, OpenStreetMap)
- **Options**: Contains options classes for service configurations
- **Extensions**: Contains extension methods for registering services

## Payment Service Example

```csharp
public class StripePaymentService : IPaymentService
{
    private readonly StripeClient _stripeClient;
    private readonly ILogger<StripePaymentService> _logger;
    
    public StripePaymentService(
        IOptions<StripeOptions> stripeOptions,
        ILogger<StripePaymentService> logger)
    {
        _stripeClient = new StripeClient(stripeOptions.Value.ApiKey);
        _logger = logger;
    }
    
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var paymentIntentService = new PaymentIntentService(_stripeClient);
            
            var paymentIntentCreateOptions = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100), // Convert to cents
                Currency = "eur",
                PaymentMethod = request.PaymentMethodId,
                Confirm = true,
                ConfirmationMethod = "automatic",
                ReturnUrl = request.ReturnUrl,
                Description = request.Description,
                Metadata = new Dictionary<string, string>
                {
                    { "RentalId", request.RentalId.ToString() },
                    { "UserId", request.UserId.ToString() }
                }
            };
            
            var paymentIntent = await paymentIntentService.CreateAsync(paymentIntentCreateOptions);
            
            return new PaymentResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                Status = paymentIntent.Status,
                ErrorMessage = paymentIntent.LastPaymentError?.Message
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing payment");
            
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    // Other methods...
}
```

## Email Service Example

```csharp
public class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly SendGridOptions _options;
    
    public SendGridEmailService(
        IOptions<SendGridOptions> options,
        ILogger<SendGridEmailService> logger)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
        _logger = logger;
    }
    
    public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = null)
    {
        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, textContent ?? htmlContent, htmlContent);
        
        var response = await _client.SendEmailAsync(msg);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        
        _logger.LogWarning("Failed to send email to {Email}. Status Code: {StatusCode}", to, response.StatusCode);
        return false;
    }
}
```
