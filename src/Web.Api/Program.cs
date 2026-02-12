using Application;
using Infrastructure;
using Infrastructure.Persistence.Database;
using Scalar.AspNetCore;
using System.Data;
using Web.Api;

var builder = WebApplication.CreateBuilder(args);

// ===================================================================
// 各レイヤーの依存性注入を設定
// ==================================================================
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddPresentation();

// アプリケーションのビルド
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 5. データベースの初期化 (app.Build() の後で実行)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 登録済みの IDbConnection を使って初期化
        var connection = services.GetRequiredService<IDbConnection>();
        DatabaseInitializer.Initialize(connection);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "データベース初期化中にエラーが発生しました。");
        throw; // 致命的なエラーとして停止させる
    }
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
