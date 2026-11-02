using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task DeleteAsync(Role role);

}