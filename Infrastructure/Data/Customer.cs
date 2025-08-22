

namespace Infrastructure.Data
{
 public class Customer
    {
        public Guid Id { get; set; }  // PK, UUID
        public required string Name { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Email { get; set; }

        public required string IMEI { get; set; }  // Unique
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public required ICollection<Loan> Loans { get; set; }
        public required DeviceStatus DeviceStatus { get; set; }
    }
}