using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Result;
using Microsoft.AspNetCore.Http;

namespace AdvertisementApp.Business.Service
{
    public class ImageStorageService : IImageStorageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024;
        private readonly string _uploadRoot;

        public ImageStorageService(string webRootPath)
        {
            _uploadRoot = Path.Combine(webRootPath, "uploads");
        }

        public async Task<IDataResult<string>> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return DataResult<string>.Ok(string.Empty);

            if (file.Length > MaxFileSize)
                return DataResult<string>.Fail("Dosya boyutu en fazla 5 MB olabilir.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return DataResult<string>.Fail("Sadece jpg, png, gif ve webp dosyaları yüklenebilir.");

            try
            {
                Directory.CreateDirectory(_uploadRoot);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var physicalPath = Path.Combine(_uploadRoot, fileName);

                await using var stream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return DataResult<string>.Ok($"/uploads/{fileName}");
            }
            catch (Exception ex)
            {
                return DataResult<string>.Fail($"Dosya kaydedilemedi: {ex.Message}");
            }
        }
    }
}
