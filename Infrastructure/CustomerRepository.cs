using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<List<Customer>> GetDefaulterAsync();
    Task<Customer?> GetByIMEIAsync(string imei);
    Task<DeviceStatus?> GetDeviceStatusIMEIAsync(string imei);
    Task<DeviceStatus?> FlagStatus(string imei, bool status);
}
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<DeviceStatus?> FlagStatus(string imei, bool status)
    {
        var deviceStatus = _context.DeviceStatuses.FirstOrDefault(c => c.IMEI == imei);
        if (deviceStatus != null)
        {
            deviceStatus.IsLocked = status;
            _context.DeviceStatuses.Update(deviceStatus);
            await _context.SaveChangesAsync();
        }
        return deviceStatus;
    }

    public async Task<List<Customer>> GetDefaulterAsync()
    {
        var defaulters = await _context.Customers
            .Include(c => c.Loans)
            .ThenInclude(l => l.RepaymentSchedules)
            .Include(c => c.DeviceStatus)
            .Where(c => c.Loans.Any(l => l.RepaymentSchedules.Any(rs => rs.IsPaid == false && rs.DueDate < DateTime.UtcNow)))
            .ToListAsync();
        return defaulters;
    }
    public async Task<Customer?> GetByIMEIAsync(string imei)
    {
        return await _context.Customers
            .Include(c => c.Loans)
            .ThenInclude(l => l.RepaymentSchedules)
            .Include(c => c.DeviceStatus)
            .FirstOrDefaultAsync(c => c.IMEI == imei);
    }

    public async Task<DeviceStatus?> GetDeviceStatusIMEIAsync(string imei)
    {
        return await _context.DeviceStatuses.FirstOrDefaultAsync(c => c.IMEI == imei);
    }
}
