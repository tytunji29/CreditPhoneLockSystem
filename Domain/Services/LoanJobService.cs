
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public interface ILoanJobService
{
    Task CheckAndUpdateLoanStatuses();
}

public class LoanJobService : ILoanJobService
{
    private readonly IUnitOfWork _unitOfWork;

    public LoanJobService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CheckAndUpdateLoanStatuses()
    {
        var loans = await _unitOfWork.Loans
            .Query() 
            .Where(l => l.Status == "Active")
            .Include(l => l.RepaymentSchedules)
            .Include(l => l.Customer)
                .ThenInclude(c => c.DeviceStatus)
            .ToListAsync();

        foreach (var loan in loans)
        {
            // Mark overdue repayment schedules
            foreach (var schedule in loan.RepaymentSchedules.Where(s => s.DueDate < DateTime.UtcNow && s.Status == "Pending"))
            {
                schedule.Status = "Overdue";
            }

            // If loan still has balance & overdue exists → lock loan + device
            if (loan.Balance > 0 && loan.RepaymentSchedules.Any(s => s.Status == "Overdue"))
            {
                loan.Status = "Locked";

                if (loan.Customer?.DeviceStatus != null)
                {
                    loan.Customer.DeviceStatus.IsLocked = true;
                    loan.Customer.DeviceStatus.LastCheckedAt = DateTime.UtcNow;
                }
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

}
