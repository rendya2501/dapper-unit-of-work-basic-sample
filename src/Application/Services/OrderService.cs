using Application.Common;
using Application.DTOs;
using Application.Repositories;
using Domain.AuditLog;
using Domain.Exceptions;
using Domain.Orders;

namespace Application.Services;

/// <summary>
/// 注文サービスの実装
/// </summary>
/// <remarks>
/// <para><strong>ビジネスロジックの実装場所</strong></para>
/// <list type="bullet">
/// <item>在庫確認・減算</item>
/// <item>注文集約の構築</item>
/// <item>トランザクション境界の管理</item>
/// <item>監査ログの記録</item>
/// </list>
/// </remarks>
/// <param name="uow">Unit of Work（DI経由で注入）</param>
public class OrderService(
    IUnitOfWork uow,
    IInventoryRepository inventory,
    IOrderRepository order,
    IAuditLogRepository auditLog)
{
    /// <inheritdoc />
    public async Task<int> CreateOrderAsync(int customerId, List<OrderItem> items)
    {
        try
        {
            await uow.BeginTransactionAsync();

            if (items.Count == 0)
                throw new BusinessRuleException("Order must have at least one item.");

            // 1. 注文集約を構築
            var orderEntity = new Order
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow
            };

            // 2. 各商品の在庫確認と注文明細追加
            foreach (var item in items)
            {
                var productEntity = await inventory.GetByProductIdAsync(item.ProductId)
                    ?? throw new NotFoundException("Product", item.ProductId.ToString());

                if (productEntity.Stock < item.Quantity)
                {
                    throw new BusinessRuleException(
                        $"Insufficient stock for {productEntity.ProductName}. " +
                        $"Available: {productEntity.Stock}, Requested: {item.Quantity}");
                }

                // 在庫減算
                await inventory.UpdateStockAsync(
                    item.ProductId,
                    productEntity.Stock - item.Quantity);

                // 注文明細を追加（集約ルートを通じて）
                orderEntity.AddDetail(item.ProductId, item.Quantity, productEntity.UnitPrice);
            }

            // 3. 注文を永続化（明細も一緒に保存される）
            var orderId = await order.CreateAsync(orderEntity);

            // 4. 監査ログ記録
            await auditLog.CreateAsync(new AuditLog
            {
                Action = "ORDER_CREATED",
                Details = $"OrderId={orderId}, CustomerId={customerId}, " +
                    $"Items={items.Count}, Total={orderEntity.TotalAmount:C}",
                CreatedAt = DateTime.UtcNow
            });

            await uow.CommitAsync();

            return orderId;
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await order.GetAllAsync();
    }

    /// <inheritdoc />
    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await order.GetByIdAsync(id)
            ?? throw new NotFoundException("Order", id.ToString());
    }
}
