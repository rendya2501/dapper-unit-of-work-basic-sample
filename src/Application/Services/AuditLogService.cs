using Application.Repositories;
using Application.Services.Abstractions;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// 監査ログサービスの実装
/// </summary>
public class AuditLogService(IAuditLogRepository repository) : IAuditLogService
{
    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100)
    {
        return await repository.GetAllAsync(limit);
    }
}
