using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdvertisementApp.Common.Helpers
{
    public class ListingDetailsDto
    {
        public decimal? Price { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Condition { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? Warranty { get; set; }
        public string? SellerType { get; set; }
        public bool Swap { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? Memory { get; set; }
        public string? Processor { get; set; }
        public string? ScreenSize { get; set; }
        public string? Mileage { get; set; }
        public string? FuelType { get; set; }
        public string? Transmission { get; set; }
        public string? DamageStatus { get; set; }
        public string? TramerStatus { get; set; }
        public string? ExpertReportUrl { get; set; }
        public string? DamageRecord { get; set; }
        public string? FloorPlanUrl { get; set; }
        public string? DeedStatus { get; set; }
        public string? RentalYield { get; set; }
        public string? VideoUrl { get; set; }
        public string? VirtualTourUrl { get; set; }
        public string? RoomCount { get; set; }
        public string? SquareMeters { get; set; }
        public string? BuildingAge { get; set; }
        public string? Floor { get; set; }
        public string? Heating { get; set; }
        public string? CoffeeType { get; set; }
        public string? Capacity { get; set; }
        public string? PowerWatts { get; set; }
        public string? BatteryHealth { get; set; }
        public string? Material { get; set; }
        public string? Size { get; set; }
        public string? SalaryMin { get; set; }
        public string? SalaryMax { get; set; }
        public string? EmploymentType { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? WorkMode { get; set; }
        public string? ServiceType { get; set; }
        public string? ServiceArea { get; set; }
        public string? PriceUnit { get; set; }
    }

    public static class ListingDetailsHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static ListingDetailsDto? Parse(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonSerializer.Deserialize<ListingDetailsDto>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public static string? Serialize(ListingDetailsDto? details)
        {
            if (details == null) return null;
            var hasValue = details.Price.HasValue
                || !string.IsNullOrWhiteSpace(details.City)
                || !string.IsNullOrWhiteSpace(details.Brand);
            if (!hasValue) return null;
            return JsonSerializer.Serialize(details, JsonOptions);
        }
    }
}
