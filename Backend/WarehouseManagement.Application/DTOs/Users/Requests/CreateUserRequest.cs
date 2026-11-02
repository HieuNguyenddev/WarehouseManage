using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WarehouseManagement.Application.DTOs.Users.Requests;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
    public string Email { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự.")]
    public string Password { get; set; } = string.Empty;
 
    [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
    public string? PhoneNumber { get; set; }
 
    [Range(1, int.MaxValue, ErrorMessage = "RoleId không hợp lệ.")]
    public int RoleId { get; set; }
 
    public bool IsActive { get; set; } = true;

    public IFormFile? Avatar { get; set; }
}