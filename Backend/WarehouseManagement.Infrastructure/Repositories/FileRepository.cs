using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Application.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Data;

namespace WarehouseManagement.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile> AddAsync(StoredFile file)
    {
        await _context.StoredFiles.AddAsync(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task<List<StoredFile>> AddRangeAsync(IReadOnlyList<StoredFile> files)
    {
        await _context.StoredFiles.AddRangeAsync(files);
        await _context.SaveChangesAsync();
        return files.ToList();
    }

    public async Task<StoredFile?> GetByIdAsync(int id)
        => await _context.StoredFiles.FindAsync(id);

    public async Task DeleteAsync(int id)
    {
        await _context.StoredFiles
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync();
    }
}
