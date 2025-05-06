using ContactForm.API.Models;
using FastEndpoints;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ContactForm.API.Services;

public class ContactFormEndpoint : Endpoint<ContactFormRequest, Result<string>>
{
    private readonly ILogger<ContactFormEndpoint> _logger;
    private readonly IConfiguration _config;

    public ContactFormEndpoint(ILogger<ContactFormEndpoint> logger, IConfiguration configuration)
    {
        _logger = logger;
        _config = configuration;
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
        var smtpInfo = _config.GetSection("smtp");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpInfo.GetValue<string>("Name"), smtpInfo.GetValue<string>("Email")));
        message.To.Add(new MailboxAddress(req.Name, req.Email));
        message.Subject = "";
        var bodyBuilder = new BodyBuilder { TextBody = "Hi, Hello", HtmlBody = "<h1>hi, Hello</h1>" };
        message.Body = bodyBuilder.ToMessageBody();
        using var client = new SmtpClient();
        var host = smtpInfo.GetValue<string>("Host");
        var port = smtpInfo.GetValue<int>("Port");
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        var Email = smtpInfo.GetValue<string>("Email");
        var Password = smtpInfo.GetValue<string>("Password");
        await client.AuthenticateAsync(Email, Password);
        _logger.LogInformation("Sending email to {0} from {1}", req.Email, smtpInfo.GetValue<string>("Email"));
        await client.SendAsync(message);
        client.Disconnect(true);
        return true;
    }
}
