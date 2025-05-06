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
        message.Subject = "Thank you for reaching out!";
        var bodyBuilder = new BodyBuilder
        {
            TextBody =
                @$"
                    Dear {req.Name},

                    Thank you for reaching out! I truly appreciate your interest and would love to assist you with your queries.

                    I will review your submission and get back to you shortly with more information. If your inquiry is urgent, please feel free to reach me at [your email address] or [your phone number].

                    Looking forward to speaking with you soon!

                    Best regards,

                    {smtpInfo.GetValue<string>("Name")}  
                    [Your Job Title]  
                    [Your Portfolio Website]  
                    [Your Social Media Links]  
                    [Your Contact Information]  
                ",
            HtmlBody =
                @$"
                    <html>
                        <body>
                            <p>Dear {req.Name},</p>
                            <p>Thank you for reaching out! I truly appreciate your interest and would love to assist you with your queries.</p>
                            <p>I will review your submission and get back to you shortly with more information. If your inquiry is urgent, please feel free to reach me at [your email address] or [your phone number].</p>
                            <p>Looking forward to speaking with you soon!</p>
                            <p>Best regards,</p>
                            <p>{smtpInfo.GetValue<string>("Name")}</p>
                            <p>[Your Job Title]</p>
                            <p>[Your Portfolio Website]</p>
                            <p>[Your Social Media Links]</p>
                            <p>[Your Contact Information]</p>
                        </body>
                    </html>",
        };
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
