using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto;

public class CreateRepaymentScheduleDto
{
    public Guid LoanId { get; set; }  // FK
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public required string Status { get; set; }  // Pending, Paid, Overdue
}

public class RepaymentScheduleResponseDto
{
    public Guid Id { get; set; }  // PK, UUID
    public Guid LoanId { get; set; }  // FK
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public required string Status { get; set; }  // Pending, Paid, Overdue
    public DateTime CreatedAt { get; set; }

}