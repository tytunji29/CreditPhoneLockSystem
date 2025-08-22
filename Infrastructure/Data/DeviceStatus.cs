namespace Infrastructure.Data
{
     public class DeviceStatus
    {
        public Guid Id { get; set; }  // PK, UUID
        public required string IMEI { get; set; }  // FK → Customer.IMEI
        public bool IsLocked { get; set; }
        public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public required Customer Customer { get; set; }
    }
}