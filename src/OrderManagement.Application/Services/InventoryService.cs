using OrderManagement.Application.Common;
using OrderManagement.Application.Repositories;
using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Application.Services;

/// <summary>
/// 在庫サービスの実装
/// </summary>
/// <remarks>
/// 在庫管理のビジネスロジックを実装します。
/// </remarks>
public class InventoryService(
    IUnitOfWork uow,
    IInventoryRepository inventory,
    IAuditLogRepository auditLog) : IInventoryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await inventory.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await inventory.GetByProductIdAsync(productId);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(string productName, int stock, decimal unitPrice)
    {
        try
        {
            uow.BeginTransaction();

            var productId = await inventory.CreateAsync(new Inventory
            {
                ProductName = productName,
                Stock = stock,
                UnitPrice = unitPrice
            });

            await auditLog.CreateAsync(new AuditLog
            {
                Action = "INVENTORY_CREATED",
                Details = $"ProductId={productId}, Name={productName}, Stock={stock}, Price={unitPrice}",
                CreatedAt = DateTime.UtcNow
            });

            uow.Commit();

            return productId;
        }
        catch
        {
            uow.Rollback();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(int productId, string productName, int stock, decimal unitPrice)
    {
        try
        {
            uow.BeginTransaction();

            _ = await inventory.GetByProductIdAsync(productId) // Ensure product exists before updating
                 ?? throw new NotFoundException("Product", productId.ToString());

            await inventory.UpdateAsync(productId, productName, stock, unitPrice);

            await auditLog.CreateAsync(new AuditLog
            {
                Action = "INVENTORY_UPDATED",
                Details = $"ProductId={productId}, Name={productName}, Stock={stock}, Price={unitPrice}",
                CreatedAt = DateTime.UtcNow
            });

            uow.Commit();
        }
        catch
        {
            uow.Rollback();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int productId)
    {
        try
        {
            uow.BeginTransaction();

            var existing = await inventory.GetByProductIdAsync(productId)
                ?? throw new NotFoundException("Product", productId.ToString());

            await inventory.DeleteAsync(productId);

            await auditLog.CreateAsync(new AuditLog
            {
                Action = "INVENTORY_DELETED",
                Details = $"ProductId={productId}, Name={existing.ProductName}",
                CreatedAt = DateTime.UtcNow
            });

            uow.Commit();
        }
        catch
        {
            uow.Rollback();
            throw;
        }
    }
}
