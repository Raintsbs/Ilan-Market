namespace AdvertisementApp.DataAccess.Seed
{
    /// <summary>Tam kategori ağacı — her API başlangıcında idempotent eklenir.</summary>
    public static class CategoryCatalogSeed
    {
        public sealed record Node(string Name, int Sort, string? Schema = null, params Node[] Children);

        private const string BrandSchema = """{"fields":[{"key":"brand","label":"Marka"},{"key":"model","label":"Model"}]}""";
        private const string VehicleSchema = """{"fields":[{"key":"brand","label":"Marka"},{"key":"model","label":"Model"},{"key":"minYear","label":"Min Yıl"},{"key":"maxYear","label":"Max Yıl"},{"key":"minMileage","label":"Min KM"},{"key":"maxMileage","label":"Max KM"},{"key":"fuelType","label":"Yakıt"}]}""";
        private const string EstateSchema = """{"fields":[{"key":"roomCount","label":"Oda"},{"key":"minSqm","label":"Min m²"},{"key":"deedStatus","label":"Tapu"}]}""";
        private const string JobSchema = """{"fields":[{"key":"employmentType","label":"Çalışma"},{"key":"experienceLevel","label":"Deneyim"},{"key":"salaryMin","label":"Min maaş"}]}""";
        private const string ServiceSchema = """{"fields":[{"key":"serviceType","label":"Hizmet"},{"key":"serviceArea","label":"Bölge"}]}""";

        private static readonly string[] CarBrands =
        [
            "Volkswagen", "Renault", "Fiat", "Ford", "Toyota", "Honda", "Hyundai", "Kia", "BMW", "Mercedes-Benz",
            "Audi", "Peugeot", "Citroën", "Opel", "Dacia", "Nissan", "Skoda", "Seat", "Volvo", "Tesla", "Chery", "TOGG", "Diğer",
        ];

        private static readonly string[] MotoBrands =
            ["Honda", "Yamaha", "Kawasaki", "BMW", "Ducati", "KTM", "Bajaj", "Mondial", "RKS", "Diğer"];

        private static readonly string[] JobSectors =
        [
            "Bilgi Teknolojileri", "Satış & Pazarlama", "Üretim", "Lojistik & Depo", "Finans & Bankacılık",
            "Sağlık", "Eğitim", "İnsan Kaynakları", "Mühendislik", "Turizm & Otelcilik", "Gıda & Restoran",
            "İnşaat", "Hukuk", "Diğer",
        ];

        private static Node Brands(int startSort, params string[] brands) =>
            new("_brands", startSort, null, brands.Select((b, i) => new Node(b, i + 1)).ToArray());

        private static Node BrandWithSeries(string brand, int sort, params string[] series) =>
            N(brand, sort, BrandSchema, series.Select((s, i) => N(s, i + 1)).ToArray());

        private static Node N(string name, int sort, string? schema = null, params Node[] children) =>
            new(name, sort, schema, children);

        private static Node[] PhoneBrandNodes() =>
        [
            BrandWithSeries("Apple", 1, "iPhone 16", "iPhone 15", "iPhone 14", "iPhone 13", "iPhone SE", "iPhone 12 ve altı", "Diğer"),
            BrandWithSeries("Samsung", 2, "Galaxy S25", "Galaxy S24", "Galaxy S23", "Galaxy A Serisi", "Galaxy Z Fold/Flip", "Galaxy Note", "Diğer"),
            BrandWithSeries("Xiaomi", 3, "Redmi Note", "Redmi", "Poco", "Mi Serisi", "Diğer"),
            BrandWithSeries("Huawei", 4, "P Serisi", "Mate Serisi", "Nova", "Diğer"),
            BrandWithSeries("Oppo", 5, "Find Serisi", "Reno Serisi", "A Serisi", "Diğer"),
            BrandWithSeries("Realme", 6, "GT Serisi", "C Serisi", "Number Serisi", "Diğer"),
            BrandWithSeries("Honor", 7, "Magic Serisi", "X Serisi", "Diğer"),
            BrandWithSeries("Vivo", 8, "X Serisi", "Y Serisi", "V Serisi", "Diğer"),
            BrandWithSeries("OnePlus", 9, "OnePlus 13", "OnePlus 12", "Nord Serisi", "Diğer"),
            BrandWithSeries("Google", 10, "Pixel 9", "Pixel 8", "Pixel 7", "Pixel A Serisi", "Diğer"),
            BrandWithSeries("Nothing", 11, "Phone (3)", "Phone (2)", "CMF Phone", "Diğer"),
            BrandWithSeries("Infinix", 12, "Note Serisi", "Hot Serisi", "Zero Serisi", "Diğer"),
            BrandWithSeries("Tecno", 13, "Camon", "Spark", "Pova", "Diğer"),
            BrandWithSeries("Nokia", 14, "G Serisi", "X Serisi", "Diğer"),
            N("General Mobile", 15), N("Casper", 16), N("Reeder", 17), N("Diğer", 99),
        ];

        private static Node[] LaptopBrandNodes() =>
        [
            BrandWithSeries("Apple", 1, "MacBook Air", "MacBook Pro", "Diğer"),
            BrandWithSeries("Dell", 2, "XPS", "Inspiron", "Latitude", "Alienware", "G Series", "Diğer"),
            BrandWithSeries("HP", 3, "Pavilion", "Envy", "Spectre", "Omen", "EliteBook", "Diğer"),
            BrandWithSeries("Lenovo", 4, "ThinkPad", "IdeaPad", "Legion", "Yoga", "LoQ", "Diğer"),
            BrandWithSeries("Asus", 5, "ZenBook", "VivoBook", "ROG", "TUF Gaming", "Diğer"),
            BrandWithSeries("Acer", 6, "Aspire", "Swift", "Predator", "Nitro", "Diğer"),
            BrandWithSeries("MSI", 7, "Katana", "Stealth", "Raider", "Creator", "Thin", "Diğer"),
            BrandWithSeries("Monster", 8, "Abra", "Tulpar", "Huma", "Semruk", "Diğer"),
            BrandWithSeries("Casper", 9, "Excalibur", "VIA", "Diğer"),
            BrandWithSeries("Huawei", 10, "MateBook", "Diğer"),
            N("Diğer", 99),
        ];

        private static Node[] DesktopBrandNodes() =>
        [
            BrandWithSeries("Apple", 1, "iMac", "Mac Mini", "Mac Studio", "Mac Pro", "Diğer"),
            BrandWithSeries("Dell", 2, "OptiPlex", "Inspiron", "XPS", "Alienware Aurora", "Diğer"),
            BrandWithSeries("HP", 3, "Pavilion", "Omen", "EliteDesk", "ProDesk", "Diğer"),
            BrandWithSeries("Lenovo", 4, "ThinkCentre", "IdeaCentre", "Legion Tower", "Diğer"),
            BrandWithSeries("Asus", 5, "ROG", "ProArt", "ExpertCenter", "Diğer"),
            BrandWithSeries("Acer", 6, "Aspire", "Predator", "Veriton", "Diğer"),
            BrandWithSeries("MSI", 7, "MAG", "MPG", "MEG", "Diğer"),
            BrandWithSeries("Monster", 8, "Hazır Sistem", "Toplama", "Diğer"),
            BrandWithSeries("Casper", 9, "Excalibur", "Diğer"),
            BrandWithSeries("Huawei", 10, "MateStation", "Diğer"),
            N("Diğer", 99),
        ];

        private static Node[] GamingPcBrandNodes() =>
        [
            BrandWithSeries("Apple", 1, "Mac Studio", "Mac Pro", "Diğer"),
            BrandWithSeries("Dell", 2, "Alienware", "G Series", "Diğer"),
            BrandWithSeries("HP", 3, "Omen", "Victus", "Diğer"),
            BrandWithSeries("Lenovo", 4, "Legion", "LOQ", "Diğer"),
            BrandWithSeries("Asus", 5, "ROG", "TUF Gaming", "Diğer"),
            BrandWithSeries("Acer", 6, "Predator", "Nitro", "Diğer"),
            BrandWithSeries("MSI", 7, "Aegis", "Infinite", "Trident", "Diğer"),
            BrandWithSeries("Monster", 8, "Abra", "Tulpar", "Huma", "Diğer"),
            BrandWithSeries("Casper", 9, "Excalibur", "Diğer"),
            N("Toplama Sistem", 10),
            N("Diğer", 99),
        ];

        private static Node[] TabletBrandNodes() =>
        [
            BrandWithSeries("Apple", 1, "iPad Pro", "iPad Air", "iPad", "iPad mini", "Diğer"),
            BrandWithSeries("Samsung", 2, "Galaxy Tab S", "Galaxy Tab A", "Galaxy Tab Active", "Diğer"),
            BrandWithSeries("Lenovo", 3, "Tab P Serisi", "Tab M Serisi", "Yoga Tab", "Diğer"),
            BrandWithSeries("Huawei", 4, "MatePad Pro", "MatePad", "Diğer"),
            BrandWithSeries("Microsoft", 5, "Surface Pro", "Surface Go", "Surface", "Diğer"),
            N("Diğer", 99),
        ];

        private static Node[] TvBrandNodes() =>
        [
            BrandWithSeries("Samsung", 1, "Neo QLED", "QLED", "Crystal UHD", "OLED", "The Frame", "Diğer"),
            BrandWithSeries("LG", 2, "OLED", "QNED", "NanoCell", "UHD", "Diğer"),
            BrandWithSeries("Sony", 3, "Bravia XR", "Bravia", "Diğer"),
            BrandWithSeries("Vestel", 4, "Smart TV", "Android TV", "Diğer"),
            BrandWithSeries("Philips", 5, "Ambilight", "OLED", "LED", "Diğer"),
            BrandWithSeries("Grundig", 6, "Smart TV", "Diğer"),
            BrandWithSeries("TCL", 7, "QLED", "Mini LED", "Android TV", "Diğer"),
            N("Diğer", 99),
        ];

        public static readonly Node[] Roots =
        [
            // ── 1 EMLAK ──
            N("Emlak", 1, null,
                N("Satılık Konut", 1, EstateSchema,
                    N("Daire", 1), N("Villa", 2), N("Müstakil Ev", 3), N("Rezidans", 4),
                    N("Yazlık", 5), N("Çiftlik Evi", 6), N("Kooperatif", 7), N("Bina", 8)),
                N("Kiralık Konut", 2, EstateSchema,
                    N("Daire", 1), N("Villa", 2), N("Müstakil Ev", 3), N("Rezidans", 4),
                    N("Yazlık", 5), N("Öğrenci Yurtları", 6), N("Günlük Kiralık", 7)),
                N("İş Yeri", 3, EstateSchema,
                    N("Ofis", 1), N("Dükkan & Mağaza", 2), N("Depo & Antrepo", 3),
                    N("Fabrika & Atölye", 4), N("Otel & Pansiyon", 5), N("Plaza Katı", 6)),
                N("Arsa", 4, EstateSchema,
                    N("İmarlı Arsa", 1), N("Tarla", 2), N("Bahçe", 3), N("Sanayi Arsası", 4)),
                N("Devremülk & Tatil", 5, EstateSchema, N("Devremülk", 1), N("Timeshare", 2)),
                N("Turistik Tesis", 6, EstateSchema, N("Otel", 1), N("Pansiyon", 2), N("Apart Otel", 3)),
                N("Projeden Konut", 7, EstateSchema, N("Daire", 1), N("Villa", 2), N("Ofis", 3)),
                N("Bina", 8, EstateSchema, N("Satılık Bina", 1), N("Kiralık Bina", 2))),

            // ── 2 VASITA (sahibinden.com uyumlu) ──
            N("Vasıta", 2, null,
                N("Otomobil", 1, VehicleSchema, Brands(1, CarBrands)),
                N("Arazi, SUV & Pick-up", 2, VehicleSchema, Brands(1, CarBrands)),
                N("Motosiklet", 3, VehicleSchema, Brands(1, MotoBrands)),
                N("Minivan, Van & Panelvan", 4, VehicleSchema,
                    N("Minivan", 1), N("Panelvan", 2), N("Van", 3), N("Diğer", 99)),
                N("Ticari Araçlar", 5, VehicleSchema,
                    N("Kamyonet", 1), N("Minibüs", 2), N("Kamyon", 3), N("Otobüs", 4), N("Çekici", 5), N("Traktör", 6)),
                N("Kiralık Araçlar", 6, VehicleSchema, N("Günlük", 1), N("Aylık", 2), N("Şoförlü", 3)),
                N("Deniz Araçları", 7, VehicleSchema,
                    N("Yat", 1), N("Tekne", 2), N("Jet Ski", 3), N("Sürat Teknesi", 4), N("Yelkenli", 5)),
                N("Hasarlı Araçlar", 8, VehicleSchema,
                    N("Otomobil", 1), N("Motosiklet", 2), N("Ticari", 3), N("SUV & Pick-up", 4)),
                N("Klasik Araçlar", 9, VehicleSchema, Brands(1, CarBrands)),
                N("Elektrikli Araçlar", 10, VehicleSchema,
                    N("Otomobil", 1), N("Motosiklet", 2), N("Scooter", 3), N("Bisiklet", 4)),
                N("Modifiye Araçlar", 11, VehicleSchema,
                    N("Otomobil", 1), N("Motosiklet", 2), N("SUV & Pick-up", 3)),
                N("Hava Araçları", 12, VehicleSchema,
                    N("Helikopter", 1), N("Uçak", 2), N("İHA & Drone", 3), N("Parapente", 4)),
                N("ATV", 13, VehicleSchema, N("Sport ATV", 1), N("Utility ATV", 2), N("Çocuk ATV", 3)),
                N("UTV", 14, VehicleSchema, N("Side-by-Side", 1), N("Çift Kişilik", 2)),
                N("Karavan", 15, VehicleSchema, N("Motokaravan", 1), N("Çekme Karavan", 2), N("Karavan Tente", 3)),
                N("Engelli Plakalı Araçlar", 16, VehicleSchema, N("Otomobil", 1), N("Minivan", 2), N("Diğer", 99))),

            // ── 3 ELEKTRONIK (sahibinden uyumlu) ──
            N("Elektronik", 3, null,
                N("Telefon", 1, BrandSchema, PhoneBrandNodes()),
                N("Bilgisayar", 2, null,
                    N("Dizüstü (Laptop)", 1, BrandSchema, LaptopBrandNodes()),
                    N("Masaüstü", 2, BrandSchema, DesktopBrandNodes()),
                    N("Oyuncu Bilgisayarı", 3, BrandSchema, GamingPcBrandNodes()),
                    N("Monitör", 4, BrandSchema,
                        BrandWithSeries("Samsung", 1, "Odyssey", "Smart Monitor", "Diğer"),
                        BrandWithSeries("LG", 2, "UltraGear", "UltraWide", "Diğer"),
                        BrandWithSeries("Asus", 3, "ROG", "TUF", "ProArt", "Diğer"),
                        BrandWithSeries("Dell", 4, "Alienware", "UltraSharp", "Diğer"),
                        N("AOC", 5), N("BenQ", 6), N("MSI", 7), N("Diğer", 99)),
                    N("Bilgisayar Bileşenleri", 5, null,
                        N("Ekran Kartı", 1), N("İşlemci", 2), N("Anakart", 3), N("RAM", 4), N("SSD & HDD", 5), N("Kasa & PSU", 6))),
                N("Tablet", 3, BrandSchema, TabletBrandNodes()),
                N("Televizyon", 4, BrandSchema,
                    TvBrandNodes().Concat([
                        N("32-43 inç", 90), N("50-55 inç", 91), N("65 inç ve üzeri", 92), N("Projeksiyon Perdesi", 93),
                    ]).ToArray()),
                N("Beyaz Eşya", 5, null,
                    N("Buzdolabı", 1, BrandSchema,
                        BrandWithSeries("Arçelik", 1, "No-Frost", "Side by Side", "Diğer"),
                        BrandWithSeries("Bosch", 2, "Serie 6", "Serie 4", "Diğer"),
                        BrandWithSeries("Samsung", 3, "Bespoke", "Diğer"),
                        BrandWithSeries("LG", 4, "InstaView", "Diğer"),
                        N("Beko", 5), N("Profilo", 6), N("Diğer", 99)),
                    N("Çamaşır Makinesi", 2, BrandSchema, N("Arçelik", 1), N("Bosch", 2), N("Samsung", 3), N("Beko", 4), N("Diğer", 99)),
                    N("Bulaşık Makinesi", 3, BrandSchema, N("Arçelik", 1), N("Bosch", 2), N("Beko", 3), N("Diğer", 99)),
                    N("Fırın & Ocak", 4, BrandSchema, N("Arçelik", 1), N("Bosch", 2), N("Siemens", 3), N("Diğer", 99)),
                    N("Kurutma Makinesi", 5), N("Derin Dondurucu", 6)),
                N("Fotoğraf & Kamera", 6, null,
                    N("DSLR & Aynasız", 1, BrandSchema,
                        BrandWithSeries("Canon", 1, "EOS R", "EOS", "Diğer"),
                        BrandWithSeries("Nikon", 2, "Z Serisi", "D Serisi", "Diğer"),
                        BrandWithSeries("Sony", 3, "Alpha 7", "Alpha 6", "Diğer"),
                        BrandWithSeries("Fujifilm", 4, "X-T", "X-S", "Diğer"),
                        N("Diğer", 99)),
                    N("Aksiyon Kamerası", 2, BrandSchema, N("GoPro", 1), N("DJI Osmo", 2), N("Insta360", 3), N("Diğer", 99)),
                    N("Kompakt", 3, BrandSchema, N("Canon", 1), N("Sony", 2), N("Panasonic", 3), N("Diğer", 99)),
                    N("Lens & Aksesuar", 4), N("Drone", 5, BrandSchema, N("DJI", 1), N("Autel", 2), N("Diğer", 99)), N("GoPro & Aksiyon", 6)),
                N("Oyun & Konsol", 7, null,
                    N("PlayStation", 1, null,
                        N("PS5", 1), N("PS5 Slim", 2), N("PS5 Pro", 3), N("PS4", 4), N("PS4 Pro", 5),
                        N("PS VR2", 6), N("PlayStation Oyunları", 7), N("Kol & Aksesuar", 8)),
                    N("Xbox", 2, null,
                        N("Xbox Series X", 1), N("Xbox Series S", 2), N("Xbox One", 3),
                        N("Xbox Oyunları", 4), N("Kol & Aksesuar", 5)),
                    N("Nintendo", 3, null,
                        N("Switch 2", 1), N("Switch OLED", 2), N("Switch", 3), N("Switch Lite", 4),
                        N("Nintendo Oyunları", 5), N("Aksesuar", 6)),
                    N("PC Oyun", 4, null,
                        N("Steam Key", 1), N("Epic Games Key", 2), N("Fiziksel Oyun", 3), N("Oyun Hesabı", 4)),
                    N("Retro Konsol", 5, null,
                        N("PlayStation 3 ve altı", 1), N("Xbox 360 ve altı", 2), N("Diğer Retro", 3)),
                    N("Oyun Aksesuarları", 6, null,
                        N("Kol & Gamepad", 1), N("Kulaklık", 2), N("Direksiyon Seti", 3), N("Oyun Koltuğu", 4), N("VR Aksesuar", 5))),
                N("Ses & Hoparlör", 8, null, N("Hoparlör", 1), N("Kulaklık", 2), N("Soundbar", 3), N("Mikrofon", 4), N("Plak & Pikap", 5)),
                N("Giyilebilir Teknoloji", 9, null, N("Akıllı Saat", 1), N("Akıllı Bileklik", 2), N("VR Gözlük", 3)),
                N("Küçük Ev Aletleri", 10, null, N("Süpürge", 1), N("Kahve Makinesi", 2), N("Airfryer", 3), N("Ütü", 4), N("Blender & Mikser", 5)),
                N("Ağ & Modem", 11, null, N("Modem & Router", 1), N("Mesh Sistem", 2), N("Ağ Kartı", 3), N("Switch", 4)),
                N("Yazıcı & Tarayıcı", 12, null, N("Yazıcı", 1), N("Tarayıcı", 2), N("3D Yazıcı", 3), N("Kartuş & Toner", 4)),
                N("Telefon Aksesuarları", 13, null, N("Kılıf", 1), N("Şarj Aleti", 2), N("Kulaklık", 3), N("Powerbank", 4)),
                N("Ev Elektroniği", 14, null,
                    N("Uydu Alıcı", 1), N("Ev Sinema Sistemi", 2), N("Projeksiyon", 3), N("Media Player", 4)),
                N("Teknik Elektronik", 15, null,
                    N("Ölçüm Cihazı", 1), N("Lehim & Havya", 2), N("Güç Kaynağı", 3), N("Test Ekipmanı", 4))),

            // ── 4 EV & YASAM ──
            N("Ev & Yaşam", 4, null,
                N("Mobilya", 1, null,
                    N("Koltuk & Kanepe", 1), N("Yatak & Baza", 2), N("Masa & Sandalye", 3),
                    N("Dolap & Gardırop", 4), N("Çocuk Odası", 5), N("Ofis Mobilyası", 6), N("Bahçe Mobilyası", 7)),
                N("Dekorasyon", 2, null, N("Ayna & Tablo", 1), N("Perde & Halı", 2), N("Aydınlatma", 3), N("Vazo & Süs", 4)),
                N("Mutfak Gereçleri", 3, null, N("Tencere & Tava", 1), N("Bardak & Tabak", 2), N("Saklama Kapları", 3)),
                N("Ev Tekstili", 4, null, N("Nevresim", 1), N("Havlu", 2), N("Yorgan & Yastık", 3)),
                N("Ev Güvenliği", 5, null, N("Kamera Sistemi", 1), N("Alarm", 2), N("Akıllı Kilit", 3)),
                N("Bahçe & Balkon", 6, null, N("Barbekü & Mangal", 1), N("Saksı & Bitki", 2), N("Havuz Malzemeleri", 3))),

            // ── 5 GIYIM ──
            N("Giyim & Aksesuar", 5, null,
                N("Kadın Giyim", 1, null, N("Elbise", 1), N("Bluz & Gömlek", 2), N("Pantolon", 3), N("Mont & Kaban", 4), N("Ayakkabı", 5)),
                N("Erkek Giyim", 2, null, N("Gömlek", 1), N("Pantolon", 2), N("Mont & Kaban", 3), N("Ayakkabı", 4), N("Takım Elbise", 5)),
                N("Çocuk Giyim", 3, null, N("Bebek", 1), N("Kız Çocuk", 2), N("Erkek Çocuk", 3), N("Ayakkabı", 4)),
                N("Aksesuar", 4, null, N("Saat", 1), N("Çanta", 2), N("Takı", 3), N("Gözlük", 4), N("Kemer & Cüzdan", 5)),
                N("Düğün & Abiye", 5, null, N("Gelinlik", 1), N("Abiye", 2), N("Smokin", 3))),

            // ── 6 ANNE & BEBEK ──
            N("Anne & Bebek", 6, null,
                N("Bebek Arabası & Puset", 1), N("Bebek Bezi & Bakım", 2), N("Bebek Odası", 3),
                N("Bebek Giyim", 4), N("Bebek Maması & Beslenme", 5), N("Oyuncak (0-3)", 6)),

            // ── 7 SPOR ──
            N("Spor & Outdoor", 7, null,
                N("Fitness & Kondisyon", 1, null, N("Koşu Bandı", 1), N("Dambıl & Ağırlık", 2), N("Bisiklet", 3)),
                N("Kamp & Doğa", 2, null, N("Çadır", 1), N("Uyku Tulumu", 2), N("Sırt Çantası", 3)),
                N("Bisiklet", 3, null, N("Dağ Bisikleti", 1), N("Şehir Bisikleti", 2), N("Elektrikli Bisiklet", 3), N("Çocuk Bisikleti", 4)),
                N("Futbol & Takım Sporları", 4), N("Kayak & Snowboard", 5), N("Dalış & Su Sporları", 6)),

            // ── 8 HOBI ──
            N("Hobi & Oyun", 8, null,
                N("Oyuncak", 1, null,
                    N("Eğitici Oyuncak", 1), N("Bebek Oyuncağı", 2), N("Aksiyon Figürü", 3),
                    N("Lego & Yapı", 4), N("Puzzle", 5), N("Drone Oyuncak", 6)),
                N("Koleksiyon", 2, null, N("Pul & Para", 1), N("Figür & Model", 2), N("Antika", 3), N("Kart & Oyun", 4)),
                N("Müzik Aletleri", 3, null, N("Gitar", 1), N("Piyano & Klavye", 2), N("Davul", 3), N("Keman", 4), N("Amfi & Pedal", 5)),
                N("Sanat Malzemeleri", 4, null, N("Resim", 1), N("El Sanatları", 2), N("Fotoğraf Baskı", 3)),
                N("Model & Maket", 5), N("Balıkçılık", 6), N("Avcılık", 7)),

            // ── 9 KITAP ──
            N("Kitap, Dergi & Film", 9, null,
                N("Kitap", 1, null, N("Roman", 1), N("Eğitim", 2), N("Çocuk Kitapları", 3), N("Yabancı Dil", 4)),
                N("Dergi", 2),
                N("Müzik", 3, null, N("Plak", 1), N("CD", 2), N("Kaset", 3), N("Enstrüman Notası", 4)),
                N("Film & Dizi", 4, null, N("DVD & Blu-ray", 1), N("Dijital", 2)),
                N("E-Kitap & Dijital", 5)),

            // ── 10 YEDEK PARCA ──
            N("Yedek Parça, Aksesuar & Tuning", 10, null,
                N("Otomobil Yedek Parça", 1, null,
                    N("Motor & Mekanik", 1), N("Kaporta & Karoser", 2), N("Elektrik & Aydınlatma", 3),
                    N("Fren & Süspansiyon", 4), N("Lastik & Jant", 5), N("Aksesuar & Tuning", 6)),
                N("Otomobil Aksesuar", 2, null,
                    N("Multimedia & Navigasyon", 1), N("Oto Bakım & Temizlik", 2), N("Güvenlik", 3), N("Bagaj & Taşıma", 4)),
                N("Motosiklet Yedek Parça", 3),
                N("Motosiklet Aksesuar", 4, null, N("Kask", 1), N("Eldiven", 2), N("Mont", 3), N("Koruma", 4)),
                N("Deniz Aracı Yedek Parça", 5),
                N("Tuning & Donanım", 6, null, N("Body Kit", 1), N("Egzoz", 2), N("Jant", 3), N("Spoiler", 4), N("Chip Tuning", 5))),

            // ── 11 BAHCE & YAPI ──
            N("Bahçe & Yapı Market", 11, null,
                N("El Aletleri", 1, null, N("Matkap", 1), N("Testere", 2), N("Tornavida Seti", 3), N("Kaynak", 4)),
                N("Boya & Badana", 2), N("Hırdavat", 3), N("Elektrik Malzemeleri", 4),
                N("Tesisat", 5), N("Bahçe Makineleri", 6, null, N("Çim Biçme", 1), N("Havuz Pompası", 2))),

            // ── 12 IS ILANLARI ──
            N("İş İlanları", 12, JobSchema,
                N("Tam Zamanlı", 1, JobSchema, Brands(1, JobSectors)),
                N("Yarı Zamanlı", 2, JobSchema, Brands(1, JobSectors)),
                N("Freelance & Uzaktan", 3, JobSchema,
                    N("Yazılım", 1), N("Tasarım", 2), N("Yazarlık & Çeviri", 3), N("Danışmanlık", 4), N("Diğer", 99)),
                N("Staj", 4, JobSchema, Brands(1, JobSectors)),
                N("Sezonluk", 5, JobSchema, N("Turizm", 1), N("Tarım", 2), N("Perakende", 3)),
                N("Yönetici", 6, JobSchema, N("Genel Müdür", 1), N("Departman Müdürü", 2), N("Takım Lideri", 3))),

            // ── 13 HIZMETLER ──
            N("Hizmetler", 13, ServiceSchema,
                N("Ev Tadilat & Usta", 1, ServiceSchema,
                    N("Boya & Badana", 1), N("Elektrik", 2), N("Tesisat", 3), N("Mobilya", 4), N("Kombi & Klima", 5), N("Cam & Alüminyum", 6)),
                N("Danışmanlık", 2, ServiceSchema, N("Hukuk", 1), N("Muhasebe", 2), N("Emlak", 3), N("İnsan Kaynakları", 4), N("Sigorta", 5)),
                N("Taşıma & Nakliye", 3, ServiceSchema, N("Ev Taşıma", 1), N("Şehirler Arası", 2), N("Parça Eşya", 3), N("Ofis Taşıma", 4)),
                N("Temizlik", 4, ServiceSchema, N("Ev Temizliği", 1), N("Ofis Temizliği", 2), N("Halı & Koltuk", 3), N("İnşaat Sonrası", 4)),
                N("Güvenlik", 5, ServiceSchema, N("Özel Güvenlik", 1), N("Alarm & Kamera", 2)),
                N("Sağlık & Bakım", 6, ServiceSchema, N("Hemşirelik", 1), N("Fizyoterapi", 2), N("Kuaför & Güzellik", 3), N("Diyetisyen", 4)),
                N("Eğitim & Özel Ders", 7, ServiceSchema, N("Matematik", 1), N("Yabancı Dil", 2), N("Müzik", 3), N("Spor", 4), N("Yazılım", 5)),
                N("Bilgisayar & Teknik Servis", 8, ServiceSchema, N("Bilgisayar Tamiri", 1), N("Telefon Tamiri", 2), N("Veri Kurtarma", 3), N("Ağ Kurulumu", 4)),
                N("Organizasyon", 9, ServiceSchema, N("Düğün", 1), N("Doğum Günü", 2), N("Kurumsal Etkinlik", 3)),
                N("Fotoğraf & Video", 10, ServiceSchema, N("Düğün Fotoğrafçısı", 1), N("Drone Çekimi", 2), N("Klip & Montaj", 3)),
                N("Oto Servis & Bakım", 11, ServiceSchema, N("Periyodik Bakım", 1), N("Kaporta & Boya", 2), N("Lastik & Jant", 3), N("Oto Elektrik", 4))),

            // ── 14 HAYVANLAR ──
            N("Hayvanlar", 14, null,
                N("Köpek", 1, null, N("Yavru", 1), N("Erkek", 2), N("Dişi", 3), N("Mamül & Aksesuar", 4)),
                N("Kedi", 2, null, N("Yavru", 1), N("Erkek", 2), N("Dişi", 3), N("Mamül & Aksesuar", 4)),
                N("Kuş", 3, null, N("Muhabbet", 1), N("Papağan", 2), N("Kanarya", 3)),
                N("Balık & Akvaryum", 4), N("Kemirgen & Tavşan", 5), N("Büyükbaş & Küçükbaş", 6), N("At & Equestrian", 7)),

            // ── 15 IS MAKINALARI ──
            N("İş Makineleri", 15, VehicleSchema,
                N("Ekskavatör", 1), N("Forklift", 2), N("Vinç", 3), N("Traktör", 4),
                N("Jeneratör & Kompresör", 5), N("Loder & Greyder", 6)),

            // ── 16 OZEL DERS (sahibinden) ──
            N("Özel Ders Verenler", 16, ServiceSchema,
                N("Lise & Üniversite", 1, ServiceSchema,
                    N("Matematik", 1), N("Fizik", 2), N("Kimya", 3), N("Biyoloji", 4), N("Edebiyat", 5), N("Tarih", 6)),
                N("İlkokul & Ortaokul", 2, ServiceSchema,
                    N("Matematik", 1), N("Türkçe", 2), N("Fen Bilimleri", 3), N("İngilizce", 4)),
                N("Yabancı Dil", 3, ServiceSchema,
                    N("İngilizce", 1), N("Almanca", 2), N("Fransızca", 3), N("Arapça", 4), N("Diğer", 99)),
                N("Müzik", 4, ServiceSchema, N("Piyano", 1), N("Gitar", 2), N("Keman", 3), N("Şan", 4), N("Diğer", 99)),
                N("Spor", 5, ServiceSchema, N("Fitness", 1), N("Yüzme", 2), N("Tenis", 3), N("Futbol", 4)),
                N("Sanat", 6, ServiceSchema, N("Resim", 1), N("Heykel", 2), N("Dans", 3)),
                N("Yazılım & Bilişim", 7, ServiceSchema, N("Programlama", 1), N("Robotik", 2), N("Grafik Tasarım", 3)),
                N("Sınav Hazırlık", 8, ServiceSchema, N("YKS", 1), N("LGS", 2), N("KPSS", 3), N("YDS", 4))),

            // ── 17 KISISEL BAKIM ──
            N("Kişisel Bakım & Kozmetik", 17, null,
                N("Parfüm & Deodorant", 1), N("Makyaj", 2), N("Cilt Bakım", 3),
                N("Saç Bakım", 4), N("Tıraş & Epilasyon", 5), N("Kişisel Bakım Cihazı", 6)),

            // ── 18 TAKI ──
            N("Takı & Mücevher", 18, null,
                N("Altın", 1, null, N("Bilezik", 1), N("Kolye", 2), N("Küpe", 3), N("Yüzük", 4), N("Set", 5)),
                N("Pırlanta", 2), N("Gümüş", 3), N("Bijuteri", 4),
                N("Saat", 5, BrandSchema, Brands(1, ["Rolex", "Omega", "Seiko", "Casio", "Apple", "Samsung", "Diğer"]))),

            // ── 19 MEDIKAL ──
            N("Medikal Ürünler", 19, null,
                N("Medikal Cihaz", 1), N("Ortopedik Ürün", 2), N("Hasta Bakım", 3),
                N("Maske & Koruyucu", 4), N("Diyabet & Tansiyon", 5)),

            // ── 20 OFIS ──
            N("Ofis & Kırtasiye", 20, null,
                N("Kırtasiye", 1), N("Ofis Mobilyası", 2), N("Yazıcı Sarf", 3),
                N("Dosyalama", 4), N("Ofis Elektroniği", 5)),

            // ── 21 YIYECEK ──
            N("Yiyecek & İçecek", 21, null,
                N("Gıda", 1), N("İçecek", 2), N("Organik Ürün", 3), N("Ev Yapımı", 4)),

            // ── 22 ANTIKA ──
            N("Antika & Sanat", 22, null,
                N("Antika Eşya", 1, null, N("Mobilya", 1), N("Saat", 2), N("Porselen & Seramik", 3), N("Gümüş & Bakır", 4)),
                N("Sanat Eseri", 2, null, N("Tablo", 1), N("Heykel", 2), N("Gravür & Litografi", 3)),
                N("Koleksiyon Parçası", 3, null, N("Para & Madalya", 1), N("Pul", 2), N("Diğer", 99)),
                N("El Sanatları", 4)),

            // ── 23 BILET ──
            N("Bilet & Etkinlik", 23, null,
                N("Konser", 1), N("Spor Müsabakası", 2), N("Tiyatro & Sinema", 3),
                N("Festival", 4), N("Ulaşım Bileti", 5), N("Diğer", 99)),

            // ── 24 DIGER ──
            N("Diğer", 24, null, N("Diğer Her Şey", 1)),
        ];
    }
}
