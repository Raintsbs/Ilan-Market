using AdvertisementApp.Business.Interface;
using AdvertisementApp.Common.Result;
using Microsoft.AspNetCore.Http;

namespace AdvertisementApp.Business.Service
{
    public class MediaStorageService : IMediaStorageService
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] VideoExtensions = { ".mp4", ".webm", ".mov" };
        private const long MaxImageSize = 5 * 1024 * 1024;
        private const long MaxVideoSize = 80 * 1024 * 1024;
        private const long MaxPanoramaSize = 15 * 1024 * 1024;

        private readonly string _uploadRoot;
        private readonly string _videoRoot;
        private readonly string _panoramaRoot;

        public MediaStorageService(string webRootPath)
        {
            _uploadRoot = Path.Combine(webRootPath, "uploads");
            _videoRoot = Path.Combine(_uploadRoot, "videos");
            _panoramaRoot = Path.Combine(_uploadRoot, "panoramas");
        }

        public Task<IDataResult<string>> SaveImageAsync(IFormFile? file) =>
            SaveFileAsync(file, _uploadRoot, ImageExtensions, MaxImageSize, "/uploads");

        public Task<IDataResult<string>> SaveVideoAsync(IFormFile? file) =>
            SaveFileAsync(file, _videoRoot, VideoExtensions, MaxVideoSize, "/uploads/videos");

        public Task<IDataResult<string>> SavePanoramaAsync(IFormFile? file) =>
            SaveFileAsync(file, _panoramaRoot, ImageExtensions, MaxPanoramaSize, "/uploads/panoramas");

        private static async Task<IDataResult<string>> SaveFileAsync(
            IFormFile? file,
            string directory,
            string[] allowedExtensions,
            long maxSize,
            string urlPrefix)
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
                Directory.CreateDirectory(directory);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var physicalPath = Path.Combine(directory, fileName);

                await using var stream = new FileStream(physicalPath, FileMode.Create);
                await file.CopyToAsync(stream);

                return DataResult<string>.Ok($"{urlPrefix}/{fileName}");
            }
            catch (Exception ex)
            {
                return DataResult<string>.Fail($"Dosya kaydedilemedi: {ex.Message}");
            }
        }
    }
}
