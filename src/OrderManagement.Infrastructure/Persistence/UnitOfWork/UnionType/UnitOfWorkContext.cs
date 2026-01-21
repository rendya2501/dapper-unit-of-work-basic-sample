using OrderManagement.Application.Common;
using OrderManagement.Application.Repositories;
using OrderManagement.Infrastructure.Repositories;

namespace OrderManagement.Infrastructure.Persistence.UnitOfWork.UnionType;

/// <summary>
/// Unit of Work コンテキストの実装
/// </summary>
internal class UnitOfWorkContext(IDbSessionAccessor accessor) : IUnitOfWorkContext
{
    /// <inheritdoc />
    public IOrderRepository Orders
        => field ??= new OrderRepository(accessor);

    /// <inheritdoc />
    public IInventoryRepository Inventory
        => field ??= new InventoryRepository(accessor);

    /// <inheritdoc />
    public IAuditLogRepository AuditLogs
        => field ??= new AuditLogRepository(accessor);
}
