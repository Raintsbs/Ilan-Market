using System.Text.Json;

namespace AdvertisementApp.Common.Helpers
{
    public static class AdvertisementImagePathsHelper
    {
        public static List<string> Parse(string? imagePath, string? imagePathsJson)
        {
            if (!string.IsNullOrWhiteSpace(imagePathsJson))
            {
                try
                {
                    var list = JsonSerializer.Deserialize<List<string>>(imagePathsJson);
                    if (list != null)
                        return list.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                }
                catch
                {
                    /* tek alan kullanılır */
                }
            }

            if (!string.IsNullOrWhiteSpace(imagePath))
                return new List<string> { imagePath };

            return new List<string>();
        }

        public static string? Serialize(IEnumerable<string> paths)
        {
            var list = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            return list.Count == 0 ? null : JsonSerializer.Serialize(list);
        }

        public static (string? Primary, string? Json) ToStorage(IEnumerable<string> paths)
        {
            var list = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (list.Count == 0) return (null, null);
            return (list[0], Serialize(list));
        }
    }
}
