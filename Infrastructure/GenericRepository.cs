using Infrastructure.Data.AppDbContext;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure;
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByPropertyAsync(Expression<Func<T, bool>> predicate);
    IQueryable<T> Query();

    Task AddAsync(T entity);
    Task AddListAsync(List<T> entity);
    void Update(T entity);
    void Remove(T entity);
}
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }
    public IQueryable<T> Query() => _dbSet.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();
    public async Task<T?> GetByPropertyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public async Task AddListAsync(List<T> entity) => await _dbSet.AddRangeAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);
}
