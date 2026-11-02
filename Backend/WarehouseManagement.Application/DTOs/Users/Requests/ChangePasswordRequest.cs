using System.ComponentModel.DataAnnotations;

namespace WarehouseManagement.Application.DTOs.Users.Requests;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 50 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

}