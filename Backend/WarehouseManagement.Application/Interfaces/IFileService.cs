using Microsoft.AspNetCore.Http;
using static WarehouseManagement.Application.DTOs.Files.FileUploadDbDto;

namespace WarehouseManagement.Application.Interfaces;

public interface IFileService
{
    Task<FileResponseDto> UploadFileAsync(IFormFile file);
    Task<List<FileResponseDto>> UploadFilesAsync(IEnumerable<IFormFile> files);
    Task<FileDownloadDto?> GetFileForDownloadAsync(int id);
    Task DeleteAsync(int id);
}
