namespace AdvertisementApp.Business.Configuration
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        /// <summary>Local | AzureBlob</summary>
        public string Provider { get; set; } = "Local";

        public string LocalUploadPath { get; set; } = "";

        public string AzureConnectionString { get; set; } = "";

        public string AzureContainer { get; set; } = "uploads";

        /// <summary>Public base URL for blob URLs (CDN or storage account).</summary>
        public string PublicBaseUrl { get; set; } = "";
    }
}
