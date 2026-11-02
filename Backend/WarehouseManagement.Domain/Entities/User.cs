namespace WarehouseManagement.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    
    public int? AvatarFileId { get; set; }
    public StoredFile? AvatarFile { get; set; }

}