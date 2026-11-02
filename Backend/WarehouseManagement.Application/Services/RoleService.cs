using WarehouseManagement.Application.DTOs.Roles.Requests;
using WarehouseManagement.Application.DTOs.Roles.Responses;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;
    public RoleService(IRoleRepository repository)
    {
        _repository = repository;
    }
 
    public async Task<List<RoleResponse>> GetAllAsync()
    {
        var roles = await _repository.GetAllAsync();
        return roles.Select(MapToResponse).ToList();
    }
 
    public async Task<RoleResponse?> GetByIdAsync(int id)
    {
        var role = await _repository.GetByIdAsync(id);
        return role == null ? null : MapToResponse(role);
    }
 
    public Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        => _repository.ExistsByNameAsync(name.Trim(), excludeId);
 
    public async Task<RoleResponse> CreateAsync(CreateRoleRequest request)
    {
        var role = new Role
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };
 
        await _repository.AddAsync(role);
        return MapToResponse(role);
    }
 
    public async Task<bool> UpdateAsync(int id, UpdateRoleRequest request)
    {
        var role = await _repository.GetByIdAsync(id);
        if (role == null) return false;
 
        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim();
        role.IsActive = request.IsActive;
 
        await _repository.UpdateAsync(role);
        return true;
    }
 
    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _repository.GetByIdAsync(id);
        if (role == null) return false;
 
        await _repository.DeleteAsync(role);
        return true;
    }
 
    private static RoleResponse MapToResponse(Role role) => new()
    {
        Id = role.Id,
        Name = role.Name,
        Description = role.Description,
        IsActive = role.IsActive
    };

}