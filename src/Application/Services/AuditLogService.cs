using Application.Repositories;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// 監査ログサービスの実装
/// </summary>
public class AuditLogService(IAuditLogRepository repository) 
{
    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100)
    {
        return await repository.GetAllAsync(limit);
    }
}
