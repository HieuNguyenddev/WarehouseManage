using Microsoft.AspNetCore.Http;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;
using static WarehouseManagement.Application.DTOs.Files.FileUploadDbDto;

namespace WarehouseManagement.Application.Services;

public class FileService : IFileService
{
    private const int MaxFilesPerUpload = 10;
    private const long MaxFileBytes = 5 * 1024 * 1024;

    private readonly IFileRepository _fileRepository;

    public FileService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<FileResponseDto> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File không hợp lệ.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var storedFile = new StoredFile
        {
            FileName = Path.GetFileName(file.FileName),
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            Content = memoryStream.ToArray(),
            Length = file.Length,
            UploadedAt = DateTime.UtcNow
        };

        var createdFile = await _fileRepository.AddAsync(storedFile);

        return MapToResponse(createdFile);
    }

    public async Task<List<FileResponseDto>> UploadFilesAsync(IEnumerable<IFormFile> files)
    {
        var selectedFiles = files?
            .Where(file => file != null)
            .ToList() ?? [];

        if (selectedFiles.Count == 0)
            throw new ArgumentException("Chưa chọn file.");

        if (selectedFiles.Count > MaxFilesPerUpload)
            throw new ArgumentException($"Chỉ được upload tối đa {MaxFilesPerUpload} file mỗi lần.");

        foreach (var file in selectedFiles)
        {
            if (file.Length == 0)
                throw new ArgumentException($"File '{file.FileName}' không hợp lệ.");

            if (file.Length > MaxFileBytes)
                throw new ArgumentException($"File '{Path.GetFileName(file.FileName)}' vượt quá 5MB.");
        }

        var storedFiles = new List<StoredFile>(selectedFiles.Count);

        foreach (var file in selectedFiles)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            storedFiles.Add(new StoredFile
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType,
                Content = memoryStream.ToArray(),
                Length = file.Length,
                UploadedAt = DateTime.UtcNow
            });
        }

        var createdFiles = await _fileRepository.AddRangeAsync(storedFiles);
        return createdFiles.Select(MapToResponse).ToList();
    }

    private static FileResponseDto MapToResponse(StoredFile file) => new()
    {
        Id = file.Id,
        FileName = file.FileName,
        Size = file.Length,
        UploadedAt = file.UploadedAt
    };

    public async Task<FileDownloadDto?> GetFileForDownloadAsync(int id)
    {
        var file = await _fileRepository.GetByIdAsync(id);
        if (file == null) return null;

        return new FileDownloadDto
        {
            Content = file.Content,
            ContentType = file.ContentType,
            FileName = file.FileName
        };
    }

    public Task DeleteAsync(int id)
        => _fileRepository.DeleteAsync(id);
}
