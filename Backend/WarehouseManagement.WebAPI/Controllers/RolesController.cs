using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.DTOs.Roles.Requests;
using WarehouseManagement.Application.DTOs.Roles.Responses;
using WarehouseManagement.Application.Interfaces;

namespace WarehouseManagement.WebAPI.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private const string RoleNotFound = "Không tìm thấy role.";
    private const string RoleNameExists = "Tên role đã tồn tại.";

    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    // GET: api/roles
    [HttpGet]
    public async Task<ActionResult<List<RoleResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET: api/roles/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleResponse>> GetById(int id)
    {
        var role = await _service.GetByIdAsync(id);

        if (role == null)
            return NotFound(RoleNotFound);

        return Ok(role);
    }

    // POST: api/roles
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request)
    {
        // 1. Kiểm tra
        if (await _service.IsNameExistsAsync(request.Name))
            return Conflict(RoleNameExists);

        // 2. Xử lý
        var role = await _service.CreateAsync(request);

        // 3. Trả kết quả (201 Created)
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    // PUT: api/roles/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoleRequest request)
    {
        // 1. Kiểm tra
        if (await _service.GetByIdAsync(id) == null)
            return NotFound(RoleNotFound);

        if (await _service.IsNameExistsAsync(request.Name, excludeId: id))
            return Conflict(RoleNameExists);

        // 2. Xử lý
        await _service.UpdateAsync(id, request);

        // 3. Trả kết quả (204 No Content)
        return NoContent();
    }

    // DELETE: api/roles/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(RoleNotFound);

        return NoContent();
    }
}