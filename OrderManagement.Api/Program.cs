using FluentValidation;
using OrderManagement.Api.Middleware;
using OrderManagement.Application.Services;
using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Infrastructure.Database;
using OrderManagement.Infrastructure.UnitOfWork;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// appsettings.json から接続文字列を取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ===== データベース初期化 =====
DatabaseInitializer.Initialize(connectionString);

// DI登録
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// UnitOfWork Factory
// UnitOfWork は毎回新しいインスタンスを生成
// 接続文字列をクロージャでキャプチャ
builder.Services.AddScoped<Func<IUnitOfWork>>(sp =>
    () => new UnitOfWork(connectionString));

// Services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// 横断的関心事：バリデーション例外処理
app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
