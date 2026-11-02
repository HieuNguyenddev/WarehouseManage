using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Data;
 
namespace WarehouseManagement.Infrastructure.Repositories;
 
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
 
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
 
    public Task<List<User>> GetAllAsync()
        => _context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .OrderBy(x => x.Id)
            .ToListAsync();
 
    public Task<User?> GetByIdAsync(int id)
        => _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);
 
    public Task<User?> GetByEmailAsync(string email)
        => _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);
 
    public Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
    {
        var query = _context.Users.Where(x => x.Email == email);
 
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
 
        return query.AnyAsync();
    }
 
    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
 
        // Nạp Role để response có RoleName
        await _context.Entry(user).Reference(x => x.Role).LoadAsync();
    }
 
    public async Task UpdateAsync(User user)
        => await _context.SaveChangesAsync();
 
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}