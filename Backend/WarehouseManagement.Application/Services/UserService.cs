using Microsoft.AspNetCore.Http;
using WarehouseManagement.Application.DTOs.Users.Requests;
using WarehouseManagement.Application.DTOs.Users.Responses;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Services;

public class UserService : IUserService
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedAvatarTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFileService _fileService;

    public UserService(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        IFileService fileService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _fileService = fileService;
    }
 
    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(MapToResponse).ToList();
    }
 
    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null ? null : MapToResponse(user);
    }
 
    public Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        => _repository.ExistsByEmailAsync(NormalizeEmail(email), excludeId);
 
    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var avatarFileId = await SaveAvatarAsync(request.Avatar);

        var user = new User()
        {
            FullName = request.FullName.Trim(),
            Email = NormalizeEmail(request.Email),
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber?.Trim(),
            RoleId = request.RoleId,
            IsActive = request.IsActive,
            AvatarFileId = avatarFileId
        };

        try
        {
            await _repository.AddAsync(user);
            return MapToResponse(user);
        }
        catch
        {
            if (avatarFileId.HasValue)
                await _fileService.DeleteAsync(avatarFileId.Value);

            throw;
        }
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return null;

        var previousAvatarId = user.AvatarFileId;
        var newAvatarId = await SaveAvatarAsync(request.Avatar);
        var saved = false;

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;

        if (newAvatarId.HasValue)
            user.AvatarFileId = newAvatarId;

        try
        {
            await _repository.UpdateAsync(user);
            saved = true;

            if (newAvatarId.HasValue && previousAvatarId.HasValue)
                await _fileService.DeleteAsync(previousAvatarId.Value);

            return MapToResponse(user);
        }
        catch
        {
            if (!saved && newAvatarId.HasValue)
                await _fileService.DeleteAsync(newAvatarId.Value);

            throw;
        }
    }
 
    public async Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return false;
 
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
 
        await _repository.UpdateAsync(user);
        return true;
    }
 
    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return false;

        var avatarFileId = user.AvatarFileId;
        await _repository.DeleteAsync(user);

        if (avatarFileId.HasValue)
            await _fileService.DeleteAsync(avatarFileId.Value);

        return true;
    }

    public async Task<AvatarUploadResult> UploadAvatarAsync(int id, IFormFile file)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
        {
            return new AvatarUploadResult(
                AvatarUploadFileStatus.UserNotFound,
                Error: "Không tìm thấy người dùng.");
        }

        if (!IsValidAvatar(file, out var error))
        {
            return new AvatarUploadResult(
                AvatarUploadFileStatus.InvalidFile,
                Error: error);
        }

        var uploaded = await SaveAvatarAsync(file);
        var previousAvatarId = user.AvatarFileId;

        user.AvatarFileId = uploaded;
        await _repository.UpdateAsync(user);

        if (previousAvatarId.HasValue)
            await _fileService.DeleteAsync(previousAvatarId.Value);

        return new AvatarUploadResult(
            AvatarUploadFileStatus.Success,
            AvatarUrl: BuildAvatarUrl(uploaded!.Value));
    }

    private async Task<int?> SaveAvatarAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        if (!IsValidAvatar(file, out var error))
            throw new ArgumentException(error);

        var uploaded = await _fileService.UploadFileAsync(file);
        return uploaded.Id;
    }

    private static bool IsValidAvatar(IFormFile? file, out string error)
    {
        if (file == null || file.Length == 0)
        {
            error = "File không hợp lệ.";
            return false;
        }

        if (file.Length > MaxAvatarBytes)
        {
            error = "Ảnh đại diện không được vượt quá 2MB.";
            return false;
        }

        if (!AllowedAvatarTypes.Contains(file.ContentType))
        {
            error = "Chỉ chấp nhận ảnh JPEG, PNG, WEBP hoặc GIF.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string BuildAvatarUrl(int fileId) => $"/api/files/{fileId}";

    private static string NormalizeEmail(string email) => email.Trim().ToLower();

    private static UserResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        RoleId = user.RoleId,
        RoleName = user.Role?.Name ?? string.Empty,
        AvatarFileId = user.AvatarFileId,
        AvatarUrl = user.AvatarFileId is int fileId
            ? BuildAvatarUrl(fileId)
            : null
    };
}