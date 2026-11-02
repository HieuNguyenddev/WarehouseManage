using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WarehouseManagement.Application.DTOs.Users.Requests;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;
 
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    public string? PhoneNumber { get; set; }
 
    [Range(1, int.MaxValue, ErrorMessage = "RoleId không hợp lệ.")]
    public int RoleId { get; set; }
 
    public bool IsActive { get; set; }

    public IFormFile? Avatar { get; set; }
}