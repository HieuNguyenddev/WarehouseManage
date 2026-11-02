using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.Application.Interfaces;
using static WarehouseManagement.Application.DTOs.Files.FileUploadDbDto;

namespace WarehouseManagement.WebAPI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52 * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] UploadFilesDto dto)
    {
        try
        {
            var result = await _fileService.UploadFilesAsync(dto.Files);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id)
    {
        var fileDto = await _fileService.GetFileForDownloadAsync(id);

        if (fileDto == null)
            return NotFound("Không tìm thấy file yêu cầu.");

        return File(fileDto.Content, fileDto.ContentType, fileDto.FileName);
    }
}
