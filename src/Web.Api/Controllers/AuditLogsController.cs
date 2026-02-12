using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Contracts.AuditLogs.Responses;

namespace Web.Api.Controllers;

/// <summary>
/// 監査ログAPIエンドポイント
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController(AuditLogService auditLogService) : ControllerBase
{
    /// <summary>
    /// すべての監査ログを取得します
    /// </summary>
    /// <param name="limit">取得件数の上限（デフォルト: 100）</param>
    /// <returns>監査ログのリスト（新しい順）</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int limit = 100)
    {
        var logs = await auditLogService.GetAllAsync(limit);

        var result = logs.Select(logs => logs.ToResponse());

        return Ok(result);
    }
}
