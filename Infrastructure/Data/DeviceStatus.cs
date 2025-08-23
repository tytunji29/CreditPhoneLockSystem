using System.Text.Json.Serialization;

namespace Infrastructure.Data
{
    public class DeviceStatus
    {
        public Guid Id { get; set; }  // PK, UUID
        public Guid CustomerId { get; set; }  // PK, UUID
        public required string IMEI { get; set; }  // FK → Customer.IMEI
        public bool IsLocked { get; set; }
        public DateTime LastCheckedAt { get; set; } = DateTime.UtcNow;

        public Customer Customer { get; set; }
    }
}