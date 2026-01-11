using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Infrastructure.UnitOfWork;

namespace OrderManagement.Application.Services;

/// <summary>
/// 在庫サービスの実装
/// </summary>
/// <remarks>
/// 在庫管理のビジネスロジックを実装します。
/// </remarks>
public class InventoryService(IUnitOfWork uow) : IInventoryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Inventory>> GetAllAsync()
    {
        return await uow.Inventory.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<Inventory?> GetByProductIdAsync(int productId)
    {
        return await uow.Inventory.GetByProductIdAsync(productId);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(string productName, int stock, decimal unitPrice)
    {
        uow.BeginTransaction();

        try
        {
            var productId = await uow.Inventory.CreateAsync(new Inventory
            {
                ProductName = productName,
                Stock = stock,
                UnitPrice = unitPrice
            });

            await uow.AuditLogs.CreateAsync(new AuditLog
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
        uow.BeginTransaction();

        try
        {
            var existing = await uow.Inventory.GetByProductIdAsync(productId)
                ?? throw new NotFoundException("Product", productId.ToString());

            await uow.Inventory.UpdateAsync(productId, productName, stock, unitPrice);

            await uow.AuditLogs.CreateAsync(new AuditLog
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
        uow.BeginTransaction();

        try
        {
            var existing = await uow.Inventory.GetByProductIdAsync(productId)
                ?? throw new InvalidOperationException($"Product {productId} not found.");

            await uow.Inventory.DeleteAsync(productId);

            await uow.AuditLogs.CreateAsync(new AuditLog
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
