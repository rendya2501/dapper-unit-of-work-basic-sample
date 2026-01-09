namespace OrderManagement.Api.Requests;

/// <summary>
/// 注文作成リクエスト
/// </summary>
/// <param name="CustomerId">顧客ID</param>
/// <param name="Items">注文アイテムリクエスト</param>
public record CreateOrderRequest(int CustomerId, List<OrderItemRequest> Items);
