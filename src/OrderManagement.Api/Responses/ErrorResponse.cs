namespace OrderManagement.Api.Responses;

/// <summary>
/// エラーレスポンス
/// </summary>
/// <param name="Error">エラーメッセージ</param>
/// <param name="Details">詳細情報（オプション）</param>
public record ErrorResponse(string Error, string? Details = null);
