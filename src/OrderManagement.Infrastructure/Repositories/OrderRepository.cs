using Dapper;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Repositories.Abstractions;
using System.Data;

namespace OrderManagement.Infrastructure.Repositories;

/// <summary>
/// 注文リポジトリの実装
/// </summary>
/// <remarks>
/// <para><strong>集約の永続化戦略</strong></para>
/// <list type="number">
/// <item>Order（親）を INSERT</item>
/// <item>生成された OrderId を取得</item>
/// <item>各 OrderDetail（子）に OrderId を設定して INSERT</item>
/// </list>
/// 
/// <para><strong>トランザクション保証</strong></para>
/// <para>
/// UnitOfWork から注入された Transaction により、
/// Order と OrderDetail の整合性が保証される。
/// </para>
/// </remarks>
/// <param name="connection">データベース接続</param>
/// <param name="transaction">トランザクション（UnitOfWork から注入）</param>
public class OrderRepository(IDbConnection connection, IDbTransaction? transaction)
    : IOrderRepository
{
    /// <inheritdoc />
    public async Task<int> CreateAsync(Order order)
    {
        // 1. 注文（親）を INSERT
        const string orderSql = """
            INSERT INTO Orders (CustomerId, CreatedAt)
            VALUES (@CustomerId, @CreatedAt);
            SELECT last_insert_rowid();
            """;

        var orderId = await connection.ExecuteScalarAsync<int>(orderSql, order, transaction);

        // 2. 注文明細（子）を一括 INSERT
        if (order.Details.Count != 0)
        {
            const string detailSql = """
                INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
                VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)
                """;

            // 各明細に OrderId を設定
            foreach (var detail in order.Details)
            {
                detail.OrderId = orderId;
            }

            await connection.ExecuteAsync(detailSql, order.Details, transaction);
        }

        return orderId;
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdAsync(int id)
    {
        // 注文を取得
        const string orderSql = "SELECT * FROM Orders WHERE Id = @Id";
        var order = await connection.QueryFirstOrDefaultAsync<Order>(
            orderSql, new { Id = id }, transaction);

        if (order == null)
            return null;

        // 注文明細を取得
        const string detailSql = "SELECT * FROM OrderDetails WHERE OrderId = @OrderId";
        var details = await connection.QueryAsync<OrderDetail>(
            detailSql, new { OrderId = id }, transaction);

        order.Details.AddRange(details);

        return order;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        // すべての注文を取得
        const string orderSql = "SELECT * FROM Orders ORDER BY CreatedAt DESC";
        var orders = (await connection.QueryAsync<Order>(orderSql, transaction: transaction))
            .ToList();

        if (orders.Count == 0)
            return orders;

        // すべての注文明細を一括取得
        var orderIds = orders.Select(o => o.Id).ToArray();
        const string detailSql = "SELECT * FROM OrderDetails WHERE OrderId IN @OrderIds";
        var allDetails = (await connection.QueryAsync<OrderDetail>(
            detailSql, new { OrderIds = orderIds }, transaction))
            .ToList();

        // 注文に明細を紐付け
        foreach (var order in orders)
        {
            var details = allDetails.Where(d => d.OrderId == order.Id);
            order.Details.AddRange(details);
        }

        return orders;
    }
}
