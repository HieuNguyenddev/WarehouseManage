using WarehouseManagement.Application.DTOs.Auth.Requests;
using WarehouseManagement.Application.DTOs.Auth.Responses;

namespace WarehouseManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
 
    // null = sai email, sai mật khẩu hoặc tài khoản bị khóa
    Task<AuthResponse?> LoginAsync(LoginRequest request);

}