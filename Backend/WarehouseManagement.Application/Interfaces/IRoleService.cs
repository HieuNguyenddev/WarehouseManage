using WarehouseManagement.Application.DTOs.Roles.Requests;
using WarehouseManagement.Application.DTOs.Roles.Responses;

namespace WarehouseManagement.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
    Task<RoleResponse?> GetByIdAsync(int id);
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request);
    Task<bool> UpdateAsync(int id, UpdateRoleRequest request);
    Task<bool> DeleteAsync(int id);

}