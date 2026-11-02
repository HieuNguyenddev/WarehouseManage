using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAt) Generate(User user);
}