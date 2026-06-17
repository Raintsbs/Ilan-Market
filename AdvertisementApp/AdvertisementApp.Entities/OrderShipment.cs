namespace AdvertisementApp.Entities
{
    public class OrderShipment
    {
        public int Id { get; set; }
        public int MarketplaceOrderId { get; set; }
        /// <summary>yurtici | aras | mng | ptt | surat</summary>
        public string CarrierCode { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public string Status { get; set; } = "preparing";
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public MarketplaceOrder Order { get; set; } = null!;
    }
}
