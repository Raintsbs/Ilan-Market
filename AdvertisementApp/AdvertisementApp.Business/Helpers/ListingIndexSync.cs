using AdvertisementApp.Common.Helpers;
using AdvertisementApp.Entities;

namespace AdvertisementApp.Business.Helpers
{
    public static class ListingIndexSync
    {
        public static void Apply(Advertisement entity)
        {
            var d = ListingDetailsHelper.Parse(entity.ListingDetailsJson);
            entity.ListingPrice = d?.Price;
            entity.ListingYear = int.TryParse(d?.Year, out var y) ? y : null;
            if (int.TryParse(d?.Mileage?.Replace(".", "").Replace(" ", ""), out var km))
                entity.ListingMileageKm = km;
            else
                entity.ListingMileageKm = null;
        }
    }
}
