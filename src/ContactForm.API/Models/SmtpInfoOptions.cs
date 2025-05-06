namespace ContactForm.API.Models;

public class SmtpInfoOptions
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
