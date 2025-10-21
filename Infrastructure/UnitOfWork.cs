using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.AppDbContext;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IGenericRepository<Loan> Loans { get; }
    IGenericRepository<AdminUser> AdminUsers { get; }
    IGenericRepository<RepaymentSchedule> RepaymentSchedules { get; }
    IGenericRepository<DeviceStatus> DeviceStatuses { get; }
     IGenericRepository<CustomerFile> CustomerFiles { get; }
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
}

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ICustomerRepository Customers { get; }
        public IGenericRepository<Loan> Loans { get; }
        public IGenericRepository<AdminUser> AdminUsers { get; }
        public IGenericRepository<RepaymentSchedule> RepaymentSchedules { get; }
        public IGenericRepository<DeviceStatus> DeviceStatuses { get; }
          public IGenericRepository<CustomerFile> CustomerFiles { get; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(_context);
            CustomerFiles = new GenericRepository<CustomerFile>(_context);
            Loans = new GenericRepository<Loan>(_context);
            AdminUsers = new GenericRepository<AdminUser>(_context);
            RepaymentSchedules = new GenericRepository<RepaymentSchedule>(_context);
            DeviceStatuses = new GenericRepository<DeviceStatus>(_context);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

