using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.UnitOfWork;

namespace OrderManagement.Application.Services;

/// <summary>
/// 監査ログサービスの実装
/// </summary>
/// <param name="uow">Unit of Work（DI経由で注入）</param>
public class AuditLogService(IUnitOfWork uow) : IAuditLogService
{
    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100)
    {
        return await uow.AuditLogs.GetAllAsync(limit);
    }
}
