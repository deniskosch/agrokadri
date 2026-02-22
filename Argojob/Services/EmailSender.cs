using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Agrojob.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer = "smtp.yandex.ru";
        private readonly string _smtpUser = "d.koshchienko@trace-x.ru";
        private readonly string _smtpPassword = "jqshlxidvxscsbtr";
        private readonly string _fromEmail = "d.koshchienko@trace-x.ru";

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Агрокадры", _fromEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();

            // Временно отключаем строгую проверку отзыва при неудаче (только для доверенных серверов!)
            client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
            {
                // Если единственная проблема — невозможность проверить отзыв (RevocationStatusUnknown),
                // и сертификат в остальном валиден — можно разрешить соединение.
                if (errors == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    // Проверим: ошибка только из-за отзыва?
                    bool onlyRevocationIssues = true;
                    foreach (var status in chain.ChainStatus)
                    {
                        if (status.Status != X509ChainStatusFlags.RevocationStatusUnknown &&
                            status.Status != X509ChainStatusFlags.OfflineRevocation)
                        {
                            onlyRevocationIssues = false;
                            break;
                        }
                    }

                    if (onlyRevocationIssues)
                        return true; // Принять сертификат
                }

                // Иначе — использовать стандартную логику (обычно false)
                return errors == SslPolicyErrors.None;
            };

            try
            {
                await client.ConnectAsync(_smtpServer, 587, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
