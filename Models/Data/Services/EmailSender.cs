using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var fromEmail = _configuration["EmailSettings:FromEmail"];
        var fromPassword = _configuration["EmailSettings:AppPassword"];
        var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

        var smtpClient = new SmtpClient(smtpHost)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(fromEmail, fromPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, "Court Case System"), // You can name it anything
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
