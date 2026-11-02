using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using WarehouseManagement.Application.DTOs.Auth.Requests;
using WarehouseManagement.Application.DTOs.Auth.Responses;
using WarehouseManagement.Application.DTOs.Users.Responses;
using WarehouseManagement.Application.Interfaces;
 
namespace WarehouseManagement.WebAPI.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string EmailExists = "Email đã được sử dụng.";
    private const string InvalidCredentials = "Email hoặc mật khẩu không đúng.";
 
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
 
    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }
 
    // POST: api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _userService.IsEmailExistsAsync(request.Email))
            return Conflict(EmailExists);
 
        var result = await _authService.RegisterAsync(request);
 
        return StatusCode(StatusCodes.Status201Created, result);
    }
 
    // POST: api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
 
        if (result == null)
            return Unauthorized(InvalidCredentials);
 
        return Ok(result);
    }
 
    // GET: api/auth/me (cần token)
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
 
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();
 
        var user = await _userService.GetByIdAsync(userId);
 
        if (user == null)
            return Unauthorized();
 
        return Ok(user);
    }
}
