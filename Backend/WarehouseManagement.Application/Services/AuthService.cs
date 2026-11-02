using WarehouseManagement.Application.DTOs.Auth.Requests;
using WarehouseManagement.Application.DTOs.Auth.Responses;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;
 
namespace WarehouseManagement.Application.Services;
 
public class AuthService : IAuthService
{
    private const string DefaultRoleName = "Staff";
 
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
 
    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }
 
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var role = await _roleRepository.GetByNameAsync(DefaultRoleName)
            ?? throw new InvalidOperationException(
                $"Chưa có role mặc định '{DefaultRoleName}'.");
 
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = NormalizeEmail(request.Email),
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim(),
            RoleId = role.Id,
            Role = role,
            IsActive = true
        };
 
        await _userRepository.AddAsync(user);
 
        return CreateAuthResponse(user);
    }
 
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email));
 
        if (user == null || !user.IsActive)
            return null;
 
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;
 
        return CreateAuthResponse(user);
    }
 
    private AuthResponse CreateAuthResponse(User user)
    {
        var (token, expiresAt) = _tokenGenerator.Generate(user);
 
        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role?.Name ?? string.Empty
        };
    }
 
    private static string NormalizeEmail(string email) => email.Trim().ToLower();
}
