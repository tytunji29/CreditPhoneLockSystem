namespace Infrastructure.Data;

public class RepaymentSchedule
{
    public Guid Id { get; set; }  // PK, UUID
    public Guid LoanId { get; set; }  // FK
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public required string Status { get; set; }  // Pending, Paid, Overdue

    // Navigation
    public Loan Loan { get; set; }
}