using Infrastructure.Data;
using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByIMEIAsync(string imei);
    Task<DeviceStatus?> GetDeviceStatusIMEIAsync(string imei);
}
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context) : base(context)
    {
        _context = context;
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
