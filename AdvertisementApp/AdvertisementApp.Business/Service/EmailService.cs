using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace AdvertisementApp.Business.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(to)) return;

            var enabled = _config.GetValue<bool>("Email:Enabled");
            if (!enabled)
            {
                _logger.LogInformation("Email disabled: To={To} Subject={Subject}", to, subject);
                return;
            }

            try
            {
                var host = _config["Email:SmtpHost"];
                var pickupDir = _config["Email:PickupDirectory"];
                SmtpClient client;
                if (!string.IsNullOrWhiteSpace(host))
                {
                    client = new SmtpClient(host, _config.GetValue("Email:SmtpPort", 587))
                    {
                        EnableSsl = _config.GetValue("Email:UseSsl", true),
                        Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                    };
                }
                else if (!string.IsNullOrWhiteSpace(pickupDir))
                {
                    if (!Path.IsPathRooted(pickupDir))
                        pickupDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, pickupDir));
                    Directory.CreateDirectory(pickupDir);
                    client = new SmtpClient
                    {
                        DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                        PickupDirectoryLocation = pickupDir,
                    };
                }
                else
                {
                    _logger.LogInformation("Email (no SMTP): To={To} Subject={Subject} Body={Body}", to, subject, htmlBody);
                    return;
                }

                using (client)
                {
                    var from = _config["Email:From"] ?? "noreply@ilanmarket.local";
                    using var message = new MailMessage(from, to.Trim(), subject, htmlBody) { IsBodyHtml = true };
                    await client.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Email send failed: To={To} Subject={Subject}", to, subject);
            }
        }
    }
}
