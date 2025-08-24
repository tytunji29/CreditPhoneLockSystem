using Domain.Dto;
using Infrastructure;
using Infrastructure.Data;

namespace Domain.Services;

public interface ICustomerService
{
    Task<ReturnObject> CreateCustomerAsync(CreateCustomerDto dto);
    Task<ReturnObject> GetAllDefaultersAsync();
    Task<ReturnObject> GetAllCustomersAsync();
    Task<ReturnObject> GetCustomerByIdAsync(Guid id);
    Task<ReturnObject> GetCustomerByIMEIAsync(string imei);
    Task<ReturnObject> GetDeviceStatusByIMEIAsync(string imei);
    Task<ReturnObject> FlagStatusByIMEI(string imei, int source);
}
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReturnObject> CreateCustomerAsync(CreateCustomerDto dto)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 1. Create Customer
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                IMEI = dto.IMEI,
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>() // avoid required member errors
            };

            await _unitOfWork.Customers.AddAsync(customer);

            // 2. Initialize Device Status
            var deviceStatus = new DeviceStatus
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                IMEI = customer.IMEI,
                IsLocked = false,
                LastCheckedAt = DateTime.UtcNow
            };
            await _unitOfWork.DeviceStatuses.AddAsync(deviceStatus);

            // 3. Create Loan
            var totalAmount = dto.AmountPaid + dto.AmountToBalance;
            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                TotalAmount = totalAmount,
                PaidAmount = dto.AmountPaid,
                Balance = dto.AmountToBalance,
                Period = dto.NumberOfRepaymentMonths,
                Status = LoanStatus.Active, // use enum/constant
                CreatedAt = DateTime.UtcNow,
                RepaymentSchedules = new List<RepaymentSchedule>()
            };
            await _unitOfWork.Loans.AddAsync(loan);

            // 4. Generate Repayment Schedules
            var installmentAmount = dto.AmountToBalance / dto.NumberOfRepaymentMonths;
            var repaymentSchedules = Enumerable.Range(1, dto.NumberOfRepaymentMonths)
                .Select(i => new RepaymentSchedule
                {
                    Id = Guid.NewGuid(),
                    LoanId = loan.Id,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    Amount = installmentAmount,
                    IsPaid = false,
                    Status = RepaymentStatus.Pending
                }).ToList();

            await _unitOfWork.RepaymentSchedules.AddListAsync(repaymentSchedules);

            // 5. Commit transaction
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6. Return Customer DTO
            var rec = new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                IMEI = customer.IMEI,
                CreatedAt = customer.CreatedAt,
                Repay = repaymentSchedules.Select(rs => new RepaymentScheduleResponseDto
                {
                    Id = rs.Id,
                    DueDate = rs.DueDate,
                    Amount = rs.Amount,
                    LoanId = rs.LoanId,
                    CreatedAt = loan.CreatedAt,
                    Status = rs.Status
                }).ToList()
            };
            return new ReturnObject { Data = rec, Message = "Customer created successfully", Status = true };
        }
        catch
        {
            await transaction.RollbackAsync();
            return new ReturnObject { Data = null, Message = "An Error Occured", Status = false };
        }
    }

    public async Task<ReturnObject> FlagStatusByIMEI(string imei, int source)
    {
        switch (source)
        {
            case 1:
                // Lock the device
                await _unitOfWork.Customers.FlagStatus(imei, true);
                break;
            case 2:
                // Unlock the device
                await _unitOfWork.Customers.FlagStatus(imei, false);
                break;
            default: return new ReturnObject { Status = false, Message = "Invalid source", Data = null };

        }
        return new ReturnObject { Status = true, Message = "Status Flagged Successfully", Data = null };
    }

    public async Task<ReturnObject> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();

        var rec = customers.Select(c => new CustomerResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            IMEI = c.IMEI,
            CreatedAt = c.CreatedAt
        });
        return new ReturnObject
        {
            Data = rec,
            Message = "Customers retrieved successfully",
            Status = true
        };
    }
    public async Task<ReturnObject> GetAllDefaultersAsync()
    {
        var customers = await _unitOfWork.Customers.GetDefaulterAsync();

        var rec = customers.Select(c => new CustomerResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            IMEI = c.IMEI,
            CreatedAt = c.CreatedAt,
            Repay = c.Loans.SelectMany(l => l.RepaymentSchedules)
                .Select(rs => new RepaymentScheduleResponseDto
                {
                    Id = rs.Id,
                    DueDate = rs.DueDate,
                    Amount = rs.Amount,
                    LoanId = rs.LoanId,
                    CreatedAt = c.CreatedAt,
                    Status = rs.Status
                }).ToList()
        });
        return new ReturnObject
        {
            Data = rec,
            Message = "Customers retrieved successfully",
            Status = true
        };
    }

    public async Task<ReturnObject> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null) return new ReturnObject { Status = false, Message = "No Record Found", Data = null };

        var rec = new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            IMEI = customer.IMEI,
            CreatedAt = customer.CreatedAt
        };
        return new ReturnObject
        {
            Data = rec,
            Message = "Customer retrieved successfully",
            Status = true
        };
    }

    public async Task<ReturnObject> GetCustomerByIMEIAsync(string imei)
    {
        var customer = await _unitOfWork.Customers.GetByIMEIAsync(imei);
        if (customer == null)
            return new ReturnObject { Status = false, Message = "No Record Found", Data = null };
        var rec = new CustomerResponseDto
        {
            Id = customer.Id,
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            IMEI = customer.IMEI,
            CreatedAt = customer.CreatedAt,
            Repay = customer.Loans.SelectMany(l => l.RepaymentSchedules)
                .Select(rs => new RepaymentScheduleResponseDto
                {
                    Id = rs.Id,
                    DueDate = rs.DueDate,
                    Amount = rs.Amount,
                    LoanId = rs.LoanId,
                    CreatedAt = customer.CreatedAt,
                    Status = rs.Status
                }).ToList()
        };
        return new ReturnObject
        {
            Data = rec,
            Message = "Customer retrieved successfully",
            Status = true
        };
    }
    public async Task<ReturnObject> GetDeviceStatusByIMEIAsync(string imei)
    {
        var customer = await _unitOfWork.Customers.GetDeviceStatusIMEIAsync(imei);
        if (customer == null)
            return new ReturnObject { Status = false, Message = "No Record Found", Data = null };

        return new ReturnObject
        {
            Data = customer.IsLocked,
            Message = "Device retrieved successfully",
            Status = true
        };
    }

    public static class LoanStatus
    {
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Locked = "Locked";
    }

    public static class RepaymentStatus
    {
        public const string Pending = "Pending";
        public const string Paid = "Paid";
        public const string Overdue = "Overdue";
    }

}