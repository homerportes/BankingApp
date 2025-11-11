using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Interfaces;
using Microsoft.Extensions.Options;
using BankingApp.Core.Domain;
using Microsoft.Extensions.Logging;
using BankingApp.Core.Domain.Settings;
using MimeKit;

namespace BankingApp.Infraestructure.Shared.Services.Email
{
    public class EmailService : IEmailService
    {

        private readonly MailSettings _settings;
        private ILogger<EmailService> Logger { get; }

        public EmailService(IOptions<MailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            Logger = logger;

        }
        public async Task SendAsync(EmailRequestDto request)
        {
            try
            {
                Logger.LogInformation($"Enviando email a {request.To} - Asunto: {request.Subject}");

                request.ToRange?.Add(request.To ?? "");
                MimeMessage email = new MimeMessage()
                {
                    Sender = MailboxAddress.Parse(_settings.EmailFrom),
                    Subject = request.Subject
                };
                foreach (var toItem in request.ToRange ?? [])
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new BodyBuilder()
                {
                    HtmlBody = request.BodyHtml
                };

                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                await smtpClient.ConnectAsync(_settings.SmptHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);

                Logger.LogInformation($"Email enviado exitosamente a {request.To}");

            }

            catch (Exception ex)
            {
                Logger.LogError($"Error al enviar email a {request.To}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Logger.LogError($"Detalle: {ex.InnerException.Message}");
                }
                throw; // Re-lanzar la excepción para que se propague

            }
        }
    }
}
