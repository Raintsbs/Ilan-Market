using AdvertisementApp.Business.Interface;
using AdvertisementApp.Business.Service;
using AdvertisementApp.DataAccess.Interface;
using AdvertisementApp.DataAccess.UnitOfWork;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdvertisementApp.Business.DependencyResolvers
{
    public static class DependencyExtension
    {
        public static void AddDependencies(this IServiceCollection services)
        {
            services.AddAutoMapper(opt =>
            {
                opt.AddMaps(Assembly.GetExecutingAssembly());
            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAdvertisementService, AdvertisementService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IPlatformService, PlatformService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            services.AddScoped<ICaptchaService, CaptchaService>();
            services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
            services.AddScoped<IAppCacheService, AppCacheService>();
            services.AddScoped<IFullTextSearchService, SqlFullTextSearchService>();
            services.AddScoped<IyzicoPaymentHelper>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<IMarketplaceOrderService, MarketplaceOrderService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IAuctionService, AuctionService>();
            services.AddScoped<ISavedSearchService, SavedSearchService>();
            services.AddScoped<ISavedSearchNotificationService, SavedSearchNotificationService>();
            services.AddScoped<IPriceAlertNotificationService, PriceAlertNotificationService>();
            services.AddScoped<IGrowthService, GrowthService>();
            services.AddScoped<ISeoService, SeoService>();
            services.AddScoped<IWebPushNotificationService, WebPushNotificationService>();
            services.AddScoped<IRealtimeNotifier, NoOpRealtimeNotifier>();
            // IImageStorageService registered in UI/API with WebRootPath
            services.AddScoped(typeof(IGenericService<,,,>), typeof(GenericService<,,,>));
        }
    }
}
