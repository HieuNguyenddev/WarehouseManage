using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Interfaces;

public interface IFileRepository
{
    Task<StoredFile> AddAsync(StoredFile file);
    Task<List<StoredFile>> AddRangeAsync(IReadOnlyList<StoredFile> files);
    Task<StoredFile?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
}
