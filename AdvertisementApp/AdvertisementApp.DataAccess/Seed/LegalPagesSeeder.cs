using AdvertisementApp.DataAccess.Context;
using AdvertisementApp.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvertisementApp.DataAccess.Seed
{
    public static class LegalPagesSeeder
    {
        private static readonly (string Slug, string Title, string Content)[] Pages =
        [
            ("kvkk", "KVKK Aydınlatma Metni", """
                <h1>KVKK Aydınlatma Metni</h1>
                <p>6698 sayılı Kişisel Verilerin Korunması Kanunu kapsamında veri sorumlusu sıfatıyla İlanMarket,
                üyelik, ilan yayınlama ve güvenli alışveriş hizmetleri için ad, e-posta, telefon, ilan içeriği
                ve işlem kayıtlarını işler.</p>
                <h2>İşlenen veriler</h2>
                <ul>
                  <li>Kimlik ve iletişim bilgileri</li>
                  <li>İlan ve mesajlaşma içerikleri</li>
                  <li>Ödeme ve sipariş kayıtları (kart bilgisi tarafımızca saklanmaz)</li>
                  <li>Telefon görüntüleme logları (güvenlik amaçlı)</li>
                </ul>
                <h2>Haklarınız</h2>
                <p>KVKK md. 11 kapsamında erişim, düzeltme, silme ve itiraz haklarınızı
                <a href="mailto:kvkk@ilanmarket.local">kvkk@ilanmarket.local</a> adresine iletebilirsiniz.</p>
                """),
            ("kullanim-kosullari", "Kullanım Koşulları", """
                <h1>Kullanım Koşulları</h1>
                <p>Platformu kullanarak aşağıdaki koşulları kabul etmiş sayılırsınız.</p>
                <ul>
                  <li>Yalnızca yasal ve doğru bilgilerle ilan verebilirsiniz.</li>
                  <li>Onaylanmamış ilanlar yayında gösterilmez.</li>
                  <li>Üçüncü kişilerin haklarını ihlal eden içerik yasaktır.</li>
                  <li>Marketplace ödemeleri yapılandırılmış ödeme sağlayıcıları üzerinden yapılır.</li>
                </ul>
                """),
            ("gizlilik", "Gizlilik Politikası", """
                <h1>Gizlilik Politikası</h1>
                <p>Kişisel verileriniz yalnızca hizmet sunumu, güvenlik ve yasal yükümlülükler için kullanılır.
                Verileriniz izniniz olmadan üçüncü taraflara satılmaz.</p>
                """),
            ("cerez", "Çerez Politikası", """
                <h1>Çerez Politikası</h1>
                <p>Oturum, tercih (tema/dil) ve analitik amaçlı çerezler kullanılabilir.
                Tarayıcı ayarlarınızdan çerezleri yönetebilirsiniz.</p>
                """),
        ];

        public static async Task EnsureAsync(AdvertisementAppDbContext context, ILogger logger)
        {
            foreach (var (slug, title, content) in Pages)
            {
                var exists = await context.StaticPages.AnyAsync(p => p.Slug == slug);
                if (exists) continue;

                context.StaticPages.Add(new StaticPage
                {
                    Slug = slug,
                    Title = title,
                    Content = content,
                    IsActive = true,
                    UpdatedTime = DateTime.UtcNow,
                });
                logger.LogInformation("Yasal sayfa eklendi: {Slug}", slug);
            }

            await context.SaveChangesAsync();
        }
    }
}
