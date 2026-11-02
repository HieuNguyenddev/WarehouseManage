namespace WarehouseManagement.Application.DTOs.Users.Responses;

public enum AvatarUploadFileStatus {Success, UserNotFound, InvalidFile}
public record AvatarUploadResult(
    AvatarUploadFileStatus Status,
    string? AvatarUrl = null,
    string? Error =null
    );