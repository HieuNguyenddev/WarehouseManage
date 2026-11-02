using Microsoft.AspNetCore.Http;
using WarehouseManagement.Application.DTOs.Users.Requests;
using WarehouseManagement.Application.DTOs.Users.Responses;

namespace WarehouseManagement.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request);
    Task<bool> DeleteAsync(int id);
    Task<AvatarUploadResult> UploadAvatarAsync(int id, IFormFile file);
}