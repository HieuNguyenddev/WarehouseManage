namespace WarehouseManagement.Application.DTOs.Files;

public record FileUploadResult(
    bool Succes,
    FileUploadDbDto.FileResponseDto? File = null,
    string? Error = null
    );