using ContactForm.API.Models;
using FastEndpoints;

namespace ContactForm.API.Services;

public class ContactFormEndpoint : Endpoint<ContactFormRequest, Result<string>>
{
    private readonly ILogger<ContactFormEndpoint> _logger;

    public ContactFormEndpoint(ILogger<ContactFormEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        _logger.LogInformation(
            "Configuring ContactFormEndpoint. {0} {1} {2}",
            Http.POST,
            "/api/contact",
            "AllowAnonymous"
        );
        Verbs(Http.POST);
        Routes("/api/contact");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ContactFormRequest req, CancellationToken ct)
    {
        await SaveRequestAsync(req);
        await SendResponseAsync(req);
        var result = Result<string>.Success("Email sent successfully");
        await SendAsync(result, cancellation: ct);
    }

    private async Task<bool> SaveRequestAsync(ContactFormRequest req)
    {
        await Task.Delay(100);
        return true;
    }

    private async Task<bool> SendResponseAsync(ContactFormRequest req)
    {
        await SendEmailAsync(req);
        return true;
    }

    private Task<bool> SendEmailAsync(ContactFormRequest req)
    {
        return Task.FromResult(true);
    }
}
