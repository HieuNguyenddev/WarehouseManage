using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.DTOs.Users.Requests;
using WarehouseManagement.Application.DTOs.Users.Responses;
using WarehouseManagement.Application.Interfaces;
using static WarehouseManagement.Application.DTOs.Files.FileUploadDbDto;
 
namespace WarehouseManagement.WebAPI.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const string UserNotFound = "Không tìm thấy người dùng.";
    private const string RoleNotFound = "Role không tồn tại.";
    private const string EmailExists = "Email đã được sử dụng.";

    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
        => Ok(await _userService.GetAllAsync());

    // GET: api/users/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
            return NotFound(UserNotFound);

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024 + 256 * 1024)]
    public async Task<ActionResult<UserResponse>> Create([FromForm] CreateUserRequest request)
    {
        // 1. Kiểm tra
        if (await _roleService.GetByIdAsync(request.RoleId) == null)
            return BadRequest(RoleNotFound);

        if (await _userService.IsEmailExistsAsync(request.Email))
            return Conflict(EmailExists);

        try
        {
            // 2. Xử lý (avatar lưu vào StoredFiles, gắn AvatarFileId)
            var user = await _userService.CreateAsync(request);

            // 3. Trả kết quả (201 Created)
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/users/1
    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024 + 256 * 1024)]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromForm] UpdateUserRequest request)
    {
        // 1. Kiểm tra
        if (await _roleService.GetByIdAsync(request.RoleId) == null)
            return BadRequest(RoleNotFound);

        try
        {
            // 2. Xử lý. Không gửi Avatar thì giữ file StoredFiles hiện tại.
            var user = await _userService.UpdateAsync(id, request);

            if (user == null)
                return NotFound(UserNotFound);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/users/1/password
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordRequest request)
    {
        var changed = await _userService.ChangePasswordAsync(id, request);

        if (!changed)
            return NotFound(UserNotFound);

        return NoContent();
    }

    // POST: api/users/1/avatar
    [HttpPost("{id:int}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(int id, [FromForm] UploadFileDto dto)
    {
        var result = await _userService.UploadAvatarAsync(id, dto.File);

        if (result.Status == AvatarUploadFileStatus.UserNotFound)
            return NotFound(result.Error ?? UserNotFound);

        if (result.Status == AvatarUploadFileStatus.InvalidFile)
            return BadRequest(result.Error);

        return Ok(new { avatarUrl = result.AvatarUrl });
    }

    // DELETE: api/users/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);

        if (!deleted)
            return NotFound(UserNotFound);

        return NoContent();
    }
}