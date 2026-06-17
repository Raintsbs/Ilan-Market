using AdvertisementApp.Business.Interface;
using Microsoft.Extensions.Configuration;

namespace AdvertisementApp.Business.Service
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IEmailService _email;
        private readonly IConfiguration _config;

        public EmailTemplateService(IEmailService email, IConfiguration config)
        {
            _email = email;
            _config = config;
        }

        public Task SendWelcomeAsync(string to, string firstName) =>
            _email.SendAsync(to, "İlanMarket'e hoş geldiniz", Layout(
                "Hoş geldiniz",
                $"""
                <p>Merhaba <strong>{Escape(firstName)}</strong>,</p>
                <p>İlanMarket hesabınız oluşturuldu. İlan verebilir, güvenli alışveriş yapabilir ve favorilerinizi takip edebilirsiniz.</p>
                <p style="margin-top:24px">{Button("İlanları keşfet", FrontendUrl("/"))}</p>
                """));

        public Task SendAdApprovedAsync(string to, string adTitle, string linkPath) =>
            _email.SendAsync(to, "İlanınız onaylandı", Layout(
                "İlan onaylandı",
                $"""
                <p><strong>{Escape(adTitle)}</strong> başlıklı ilanınız incelendi ve yayına alındı.</p>
                <p style="margin-top:24px">{Button("İlanı görüntüle", FrontendUrl(linkPath))}</p>
                """));

        public Task SendPaymentConfirmedAsync(string to, string description, decimal amount, string? linkPath) =>
            _email.SendAsync(to, "Ödemeniz alındı", Layout(
                "Ödeme onayı",
                $"""
                <p>{Escape(description)}</p>
                <p style="font-size:20px;font-weight:600;margin:16px 0">{amount:N2} TL</p>
                {(linkPath != null ? $"""<p style="margin-top:24px">{Button("Detayları gör", FrontendUrl(linkPath))}</p>""" : "")}
                """));

        public Task SendOrderNotificationAsync(string to, string title, string body, string? linkPath) =>
            _email.SendAsync(to, $"İlanMarket — {title}", Layout(
                title,
                $"""
                <p>{body}</p>
                {(linkPath != null ? $"""<p style="margin-top:24px">{Button("Görüntüle", FrontendUrl(linkPath))}</p>""" : "")}
                """));

        public Task SendListingQuestionEmailAsync(string to, string adTitle, string question, string linkPath) =>
            _email.SendAsync(to, "İlanınıza yeni soru", Layout(
                "Yeni soru",
                $"""
                <p><strong>{Escape(adTitle)}</strong> ilanınıza soru geldi:</p>
                <blockquote style="margin:16px 0;padding:12px 16px;border-left:4px solid #2563eb;background:#f8fafc">{Escape(question)}</blockquote>
                <p style="margin-top:24px">{Button("Yanıtla", FrontendUrl(linkPath))}</p>
                """));

        public Task SendQuestionAnsweredEmailAsync(string to, string adTitle, string answer, string linkPath) =>
            _email.SendAsync(to, "Sorunuz yanıtlandı", Layout(
                "Sorunuza yanıt",
                $"""
                <p><strong>{Escape(adTitle)}</strong> ilanındaki sorunuza yanıt verildi:</p>
                <blockquote style="margin:16px 0;padding:12px 16px;border-left:4px solid #10b981;background:#f0fdf4">{Escape(answer)}</blockquote>
                <p style="margin-top:24px">{Button("İlanı gör", FrontendUrl(linkPath))}</p>
                """));

        public Task SendFollowedSellerListingEmailAsync(string to, string sellerName, string adTitle, string linkPath) =>
            _email.SendAsync(to, "Takip ettiğiniz satıcı yeni ilan verdi", Layout(
                "Yeni ilan",
                $"""
                <p><strong>{Escape(sellerName)}</strong> yeni bir ilan yayınladı:</p>
                <p style="font-size:18px;font-weight:600">{Escape(adTitle)}</p>
                <p style="margin-top:24px">{Button("İlanı incele", FrontendUrl(linkPath))}</p>
                """));

        public Task SendSellerPayoutEmailAsync(string to, decimal amount, string? note, string linkPath) =>
            _email.SendAsync(to, "Satıcı ödemeniz işlendi", Layout(
                "Ödeme yapıldı",
                $"""
                <p>Hesabınıza <strong>{amount:N2} TL</strong> tutarında satıcı ödemesi işlendi.</p>
                {(string.IsNullOrWhiteSpace(note) ? "" : $"<p style=\"color:#64748b\">Not: {Escape(note)}</p>")}
                <p style="margin-top:24px">{Button("Sipariş detayı", FrontendUrl(linkPath))}</p>
                """));

        private string FrontendUrl(string path)
        {
            var baseUrl = (_config["App:FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(path) || path == "/") return baseUrl;
            return path.StartsWith("http") ? path : $"{baseUrl}{(path.StartsWith('/') ? path : "/" + path)}";
        }

        private static string Escape(string? value) =>
            System.Net.WebUtility.HtmlEncode(value ?? "");

        private static string Button(string label, string href) =>
            $"""<a href="{href}" style="display:inline-block;background:#2563eb;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:600">{Escape(label)}</a>""";

        private static string Layout(string heading, string bodyHtml) =>
            $"""
            <!DOCTYPE html>
            <html lang="tr">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
            <body style="margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#0f172a">
              <table width="100%" cellpadding="0" cellspacing="0" style="padding:32px 16px">
                <tr><td align="center">
                  <table width="100%" style="max-width:560px;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(15,23,42,.08)">
                    <tr><td style="background:#0f172a;color:#fff;padding:20px 24px;font-size:18px;font-weight:700">İlanMarket</td></tr>
                    <tr><td style="padding:28px 24px">
                      <h1 style="margin:0 0 16px;font-size:22px">{Escape(heading)}</h1>
                      {bodyHtml}
                    </td></tr>
                    <tr><td style="padding:16px 24px;background:#f8fafc;font-size:12px;color:#64748b">
                      Bu e-posta İlanMarket tarafından gönderilmiştir.
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}
