namespace Infrastructure.Data
{
    public class Loan
    {
        public Guid Id { get; set; }  // PK, UUID
        public Guid CustomerId { get; set; }  // FK
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public int Period { get; set; }
        public required string Status { get; set; }  // Active, Completed, Locked
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Customer Customer { get; set; }
        public required ICollection<RepaymentSchedule> RepaymentSchedules { get; set; }
    }
}