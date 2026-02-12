using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Contracts.Inventories.Requests;
using Web.Api.Contracts.Inventories.Responses;

namespace Web.Api.Controllers;

/// <summary>
/// 在庫管理APIエンドポイント
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController(InventoryService inventoryService) : ControllerBase
{
    /// <summary>
    /// すべての在庫を取得します
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InventoryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var inventories = await inventoryService.GetAllAsync();

        var result = inventories.Select(i => i.ToResponse());

        return Ok(result);
    }

    /// <summary>
    /// 商品IDを指定して在庫を取得します
    /// </summary>
    [HttpGet("{productId}", Name = "GetByProductId")]
    [ProducesResponseType(typeof(InventoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProductId(int productId)
    {
        var inventory = await inventoryService.GetByProductIdAsync(productId);

        if (inventory == null)
        {
            return NotFound(new { Error = $"Product {productId} not found." });
        }

        var result = inventory.ToResponse();

        return Ok(result);
    }

    /// <summary>
    /// 在庫を作成します
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateInventoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryRequest request)
    {
        var productId = await inventoryService.CreateAsync(
            request.ProductName, request.Stock, request.UnitPrice);

        return CreatedAtAction(
            nameof(GetByProductId),
            new { productId },
            new CreateInventoryResponse(productId));
    }

    /// <summary>
    /// 在庫を更新します
    /// </summary>
    [HttpPut("{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateInventoryRequest request)
    {
        await inventoryService.UpdateAsync(
            productId, request.ProductName, request.Stock, request.UnitPrice);

        return NoContent();
    }

    /// <summary>
    /// 在庫を削除します
    /// </summary>
    [HttpDelete("{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int productId)
    {
        await inventoryService.DeleteAsync(productId);

        return NoContent();
    }
}