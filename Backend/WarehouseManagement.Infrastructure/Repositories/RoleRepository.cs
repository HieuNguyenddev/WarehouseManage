using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;
 
    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }
 
    public Task<List<Role>> GetAllAsync()
        => _context.Roles.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
 
    public Task<Role?> GetByIdAsync(int id)
        => _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
 
    public Task<Role?> GetByNameAsync(string name)
        => _context.Roles.FirstOrDefaultAsync(x => x.Name == name);
 
    public Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _context.Roles.Where(x => x.Name.ToLower() == name.ToLower());
 
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
 
        return query.AnyAsync();
    }
 
    public async Task AddAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
    }
 
    public async Task UpdateAsync(Role role)
        => await _context.SaveChangesAsync();
 
    public async Task DeleteAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

}