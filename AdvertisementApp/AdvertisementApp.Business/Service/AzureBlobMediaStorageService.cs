using AdvertisementApp.Business.Interface;
using AdvertisementApp.Business.Configuration;
using AdvertisementApp.Common.Result;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AdvertisementApp.Business.Service
{
    public class AzureBlobMediaStorageService : IMediaStorageService
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov" };
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxVideoSize = 80 * 1024 * 1024;
        private const long MaxPanoramaSize = 15 * 1024 * 1024;

        private readonly BlobContainerClient _container;
        private readonly string _publicBase;

        public AzureBlobMediaStorageService(IOptions<StorageOptions> options)
        {
            var cfg = options.Value;
            if (string.IsNullOrWhiteSpace(cfg.AzureConnectionString))
                throw new InvalidOperationException("Storage:AzureConnectionString yapılandırılmamış.");

            _container = new BlobContainerClient(cfg.AzureConnectionString, cfg.AzureContainer);
            _container.CreateIfNotExists(PublicAccessType.Blob);
            _publicBase = cfg.PublicBaseUrl.TrimEnd('/');
        }

        public Task<IDataResult<string>> SaveImageAsync(IFormFile? file) =>
            SaveAsync(file, "images", ImageExtensions, MaxImageSize);

        public Task<IDataResult<string>> SaveVideoAsync(IFormFile? file) =>
            SaveAsync(file, "videos", VideoExtensions, MaxVideoSize);

        public Task<IDataResult<string>> SavePanoramaAsync(IFormFile? file) =>
            SaveAsync(file, "panoramas", ImageExtensions, MaxPanoramaSize);

        private async Task<IDataResult<string>> SaveAsync(
            IFormFile? file,
            string folder,
            string[] allowedExtensions,
            long maxSize)
        {
            if (file == null || file.Length == 0)
                return DataResult<string>.Ok(string.Empty);

            if (file.Length > maxSize)
                return DataResult<string>.Fail($"Dosya boyutu en fazla {maxSize / (1024 * 1024)} MB olabilir.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return DataResult<string>.Fail($"İzin verilen uzantılar: {string.Join(", ", allowedExtensions)}");

            try
            {
                var blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
                var blob = _container.GetBlobClient(blobName);
                await using var stream = file.OpenReadStream();
                await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                var url = string.IsNullOrWhiteSpace(_publicBase)
                    ? blob.Uri.ToString()
                    : $"{_publicBase}/{blobName}";
                return DataResult<string>.Ok(url);
            }
            catch (Exception ex)
            {
                return DataResult<string>.Fail($"Dosya yüklenemedi: {ex.Message}");
            }
        }
    }
}
