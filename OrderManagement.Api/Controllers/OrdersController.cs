using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Requests;
using OrderManagement.Api.Responses;
using OrderManagement.Application.Models;
using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.UnitOfWork;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// 注文関連のAPIエンドポイント
/// </summary>
/// <param name="orderService">注文サービス</param>
/// <param name="unitOfWorkFactory">UnitOfWork ファクトリ</param>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    IOrderService orderService,
    Func<IUnitOfWork> unitOfWorkFactory) : ControllerBase
{
    /// <summary>
    /// 注文を作成します
    /// </summary>
    /// <param name="request">注文作成リクエスト</param>
    /// <returns>作成された注文のID</returns>
    /// <response code="200">注文が正常に作成されました</response>
    /// <response code="400">リクエストが不正です</response>
    /// <response code="500">内部サーバーエラー</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // リクエストモデルを Application 層のモデルに変換
            var items = request.Items
                .Select(i => new OrderItem(i.ProductId, i.Quantity))
                .ToList();

            var orderId = await orderService.CreateOrderAsync(request.CustomerId, items);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = orderId },
                new CreateOrderResponse(orderId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse("Internal server error", ex.Message));
        }
    }

    /// <summary>
    /// すべての注文を取得します
    /// </summary>
    /// <returns>注文のリスト</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Order>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders()
    {
        using var uow = unitOfWorkFactory();
        var orders = await uow.Orders.GetAllAsync();
        return Ok(orders);
    }

    /// <summary>
    /// IDを指定して注文を取得します
    /// </summary>
    /// <param name="id">注文ID</param>
    /// <returns>注文</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        using var uow = unitOfWorkFactory();
        var order = await uow.Orders.GetByIdAsync(id);

        if (order == null)
            return NotFound(new { Error = $"Order {id} not found." });

        return Ok(order);
    }

    /// <summary>
    /// すべての監査ログを取得します
    /// </summary>
    /// <param name="limit">取得件数の上限</param>
    /// <returns>監査ログのリスト</returns>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(IEnumerable<AuditLog>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 100)
    {
        using var uow = unitOfWorkFactory();
        var logs = await uow.AuditLogs.GetAllAsync(limit);
        return Ok(logs);
    }
}
