using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Application.DTOs.Roles.Requests;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "Tên role không được để trống.")]
    [StringLength(50, ErrorMessage = "Tên role tối đa 50 ký tự.")]
    public string Name { get; set; } = string.Empty;
 
    [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự.")]
    public string? Description { get; set; }
 
    public bool IsActive { get; set; } = true;

}