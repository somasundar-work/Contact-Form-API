using System.Text.Json;
using ContactForm.API.Constants;
using ContactForm.API.Models;
using FastEndpoints;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ContactForm.API.Services;

public class ContactFormEndpoint : Endpoint<ContactFormRequest, Result<string>>
{
    private readonly ILogger<ContactFormEndpoint> _logger;
    private readonly SmtpInfoOptions _smtpInfo;
    private readonly ProfileOptions _profile;

    public ContactFormEndpoint(
        ILogger<ContactFormEndpoint> logger,
        IOptions<SmtpInfoOptions> smtpInfo,
        IOptions<ProfileOptions> profile
    )
    {
        _logger = logger;
        _smtpInfo = smtpInfo.Value;
        _profile = profile.Value;
    }

    public override void Configure()
    {
        _logger.LogInformation(
            "Configuring ContactFormEndpoint. {0} {1} {2} {3}",
            Http.POST,
            "/api/contact",
            "AllowAnonymous",
            "Version(1, 0)"
        );
        Verbs(Http.POST);
        Routes("/api/contact");
        AllowAnonymous();
        Version(1, 0);
        Description(x =>
            x.Produces<string>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .Produces<string>(StatusCodes.Status500InternalServerError)
                .WithTags("Contact Form API V1")
        );
    }

    public override async Task HandleAsync(ContactFormRequest req, CancellationToken ct)
    {
        _logger.LogInformation("Handling request for ContactFormEndpoint.");
        try
        {
            _logger.LogInformation("Saving request...");
            await SaveRequestAsync(req);
            _logger.LogInformation("Request saved successfully.");

            _logger.LogInformation("Sending response...");
            await SendResponseAsync(req);
            _logger.LogInformation("Response sent successfully.");

            var result = Result<string>.Success("Email sent successfully");
            _logger.LogInformation("Sending success result to client.");
            await SendAsync(result, cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the request.");
            var result = Result<string>.Failure("An error occurred while processing your request.");
            await SendAsync(result, cancellation: ct);
        }
    }

    private async Task<bool> SaveRequestAsync(ContactFormRequest req)
    {
        _logger.LogInformation("Simulating saving request to database.");
        await Task.Delay(100);
        _logger.LogInformation("Request saved to database.");
        return true;
    }

    private async Task<bool> SendResponseAsync(ContactFormRequest req)
    {
        _logger.LogInformation("Preparing to send email to {0} from {1}", req.Email, _smtpInfo.Email);
        _logger.LogInformation("SMTP Info: {0}", JsonSerializer.Serialize(_smtpInfo));
        _logger.LogInformation("Request: {0}", JsonSerializer.Serialize(req));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpInfo.Name, _smtpInfo.Email));
        message.To.Add(new MailboxAddress(req.Name, req.Email));
        message.Bcc.Add(new MailboxAddress(_profile.Name, _profile.Email));
        message.Subject = AppConstant.Subject;
        _logger.LogInformation("Email subject set to: {0}", AppConstant.Subject);
        _logger.LogInformation("Email message created.");

        _logger.LogInformation("Loading email templates...");
        var textBodyTemplate = await GetTemplateAsync(AppConstant.Template.ContactFormResponsePlainText);
        var htmlBodyTemplate = await GetTemplateAsync(AppConstant.Template.ContactFormResponseHtml);

        _logger.LogInformation("Replacing placeholders in templates...");
        var textBody = ReplacePlaceholders(textBodyTemplate, req);
        var htmlBody = ReplacePlaceholders(htmlBodyTemplate, req);

        var bodyBuilder = new BodyBuilder { TextBody = textBody, HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        _logger.LogInformation("Connecting to SMTP server...");
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpInfo.Host, _smtpInfo.Port, SecureSocketOptions.StartTls);
        _logger.LogInformation("Connected to SMTP server.");

        _logger.LogInformation("Authenticating SMTP client...");
        await client.AuthenticateAsync(_smtpInfo.Email, _smtpInfo.Password);
        _logger.LogInformation("SMTP client authenticated.");

        _logger.LogInformation("Sending email...");
        await client.SendAsync(message);
        _logger.LogInformation("Email sent successfully.");

        client.Disconnect(true);
        _logger.LogInformation("Disconnected from SMTP server.");
        return true;
    }

    private async Task<string> GetTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        _logger.LogInformation("Reading template file: {0}", templatePath);
        using var reader = new StreamReader(templatePath);
        var content = await reader.ReadToEndAsync();
        _logger.LogInformation("Template file read successfully.");
        return content;
    }

    private string ReplacePlaceholders(string template, ContactFormRequest req)
    {
        _logger.LogInformation("Replacing placeholders in template.");
        _logger.LogInformation("Template: {0}", template);
        _logger.LogInformation("Request: {0}", JsonSerializer.Serialize(req));
        _logger.LogInformation("Profile: {0}", JsonSerializer.Serialize(_profile));

        return template
            .Replace(AppConstant.SubmitterPlaceholder.Name, req.Name)
            .Replace(AppConstant.SubmitterPlaceholder.Email, req.Email)
            .Replace(AppConstant.SubmitterPlaceholder.Message, req.Message)
            .Replace(AppConstant.ProfilePlaceholder.Name, _profile.Name)
            .Replace(AppConstant.ProfilePlaceholder.Email, _profile.Email)
            .Replace(AppConstant.ProfilePlaceholder.Contact, _profile.Contact)
            .Replace(AppConstant.ProfilePlaceholder.Website, _profile.Website)
            .Replace(AppConstant.ProfilePlaceholder.Github, _profile.Github)
            .Replace(AppConstant.ProfilePlaceholder.LinkedIn, _profile.LinkedIn)
            .Replace(AppConstant.ProfilePlaceholder.Whatsapp, _profile.Whatsapp)
            .Replace(AppConstant.ProfilePlaceholder.Address, _profile.Address);
    }
}
