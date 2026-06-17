namespace AdvertisementApp.Entities
{
    public enum MarketplaceOrderStatus
    {
        AwaitingPayment = 0,
        PaidEscrow = 1,
        Preparing = 2,
        Shipped = 3,
        Delivered = 4,
        Completed = 5,
        Cancelled = 6,
        Disputed = 7,
    }

    public class MarketplaceOrder
    {
        public int Id { get; set; }
        public int AdvertisementId { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }
        public decimal Amount { get; set; }
        /// <summary>card | param_guvende</summary>
        public string PaymentMethod { get; set; } = "param_guvende";
        public MarketplaceOrderStatus Status { get; set; } = MarketplaceOrderStatus.AwaitingPayment;
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? StripeSessionId { get; set; }
        public string? IyzicoToken { get; set; }
        public string? DisputeReason { get; set; }
        public DateTime? DisputedAt { get; set; }
        public int? DisputedByUserId { get; set; }
        public string? DisputeResolutionNote { get; set; }
        public DateTime? DisputeResolvedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string? RefundNote { get; set; }
        public DateTime? SellerPaidOutAt { get; set; }
        public string? SellerPayoutNote { get; set; }

        public Advertisement Advertisement { get; set; } = null!;
        public OrderShipment? Shipment { get; set; }
    }
}
