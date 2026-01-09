using FluentValidation;

namespace OrderManagement.Api.Middleware;

/// <summary>
/// バリデーション例外を処理するミドルウェア
/// </summary>
/// <remarks>
/// <para><strong>横断的関心事 (Cross-Cutting Concerns) とは</strong></para>
/// <para>
/// 複数のレイヤーやコンポーネントに共通して必要となる機能のこと。
/// 例: ロギング、エラーハンドリング、バリデーション、認証・認可など。
/// </para>
/// 
/// <para><strong>ミドルウェアで制御するメリット</strong></para>
/// <list type="bullet">
/// <item>各Controllerで try-catch を書く必要がない</item>
/// <item>エラーレスポンス形式を統一できる</item>
/// <item>保守性・テスタビリティが向上</item>
/// </list>
/// </remarks>
public class ValidationExceptionMiddleware(
    RequestDelegate next,
    ILogger<ValidationExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business logic error");
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        var errors = ex.Errors.Select(e => new
        {
            Property = e.PropertyName,
            Error = e.ErrorMessage
        });

        return context.Response.WriteAsJsonAsync(new
        {
            Type = "ValidationError",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Errors = errors
        });
    }

    private static Task HandleBusinessExceptionAsync(HttpContext context, InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            Type = "BusinessError",
            Title = "Business logic validation failed.",
            Status = 400,
            Error = ex.Message
        });
    }

    private static Task HandleGenericExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new
        {
            Type = "InternalServerError",
            Title = "An unexpected error occurred.",
            Status = 500,
            Error = "Please contact support if the problem persists."
        });
    }
}
