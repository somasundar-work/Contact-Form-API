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
        _logger.LogInformation("Sending email to {0} from {1}", req.Email, _smtpInfo.Email);
        _logger.LogInformation("SMTP Info: {0}", JsonSerializer.Serialize(_smtpInfo));
        _logger.LogInformation("Request: {0}", JsonSerializer.Serialize(req));
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpInfo.Name, _smtpInfo.Email));
        message.To.Add(new MailboxAddress(req.Name, req.Email));
        message.Bcc.Add(new MailboxAddress(_profile.Name, _profile.Email));
        message.Subject = "Thank you for reaching out!";
        var textBodyTemplate = await GetTemplateAsync(Constant.Template.ContactFormResponsePlainText);
        var htmlBodyTemplate = await GetTemplateAsync(Constant.Template.ContactFormResponseHtml);
        var textBody = ReplacePlaceholders(textBodyTemplate, req);
        var htmlBody = ReplacePlaceholders(htmlBodyTemplate, req);
        var bodyBuilder = new BodyBuilder { TextBody = textBody, HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();
        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpInfo.Host, _smtpInfo.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_smtpInfo.Email, _smtpInfo.Password);
        await client.SendAsync(message);
        client.Disconnect(true);
        return true;
    }

    private static async Task<string> GetTemplateAsync(string templateName)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateName);
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }
        using var reader = new StreamReader(templatePath);
        return await reader.ReadToEndAsync();
    }

    private string ReplacePlaceholders(string template, ContactFormRequest req)
    {
        _logger.LogInformation("Replacing placeholders in template");
        _logger.LogInformation("Template: {0}", template);
        _logger.LogInformation("Request: {0}", JsonSerializer.Serialize(req));
        _logger.LogInformation("Profile: {0}", JsonSerializer.Serialize(_profile));
        return template
            .Replace(Constant.SubmitterPlaceholder.Name, req.Name)
            .Replace(Constant.SubmitterPlaceholder.Email, req.Email)
            .Replace(Constant.SubmitterPlaceholder.Message, req.Message)
            .Replace(Constant.ProfilePlaceholder.Name, _profile.Name)
            .Replace(Constant.ProfilePlaceholder.Email, _profile.Email)
            .Replace(Constant.ProfilePlaceholder.Contact, _profile.Contact)
            .Replace(Constant.ProfilePlaceholder.Website, _profile.Website)
            .Replace(Constant.ProfilePlaceholder.Github, _profile.Github)
            .Replace(Constant.ProfilePlaceholder.LinkedIn, _profile.LinkedIn)
            .Replace(Constant.ProfilePlaceholder.Whatsapp, _profile.Whatsapp)
            .Replace(Constant.ProfilePlaceholder.Address, _profile.Address);
        ;
    }
}
