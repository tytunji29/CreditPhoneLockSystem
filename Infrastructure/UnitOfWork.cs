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
    IGenericRepository<Customer> Customers { get; }
    IGenericRepository<Loan> Loans { get; }
    IGenericRepository<RepaymentSchedule> RepaymentSchedules { get; }
    IGenericRepository<DeviceStatus> DeviceStatuses { get; }
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync();
}


public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IGenericRepository<Customer> Customers { get; }
    public IGenericRepository<Loan> Loans { get; }
    public IGenericRepository<RepaymentSchedule> RepaymentSchedules { get; }
    public IGenericRepository<DeviceStatus> DeviceStatuses { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Customers = new GenericRepository<Customer>(_context);
        Loans = new GenericRepository<Loan>(_context);
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