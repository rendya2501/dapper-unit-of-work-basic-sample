using FluentValidation;
using Microsoft.Data.Sqlite;
using OrderManagement.Api.Filters;
using OrderManagement.Api.Middleware;
using OrderManagement.Application.Common;
using OrderManagement.Application.Repositories;
using OrderManagement.Application.Services;
using OrderManagement.Application.Services.Abstractions;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Database;
using OrderManagement.Infrastructure.Persistence.Repositories;
using OrderManagement.Infrastructure.Persistence.UnitOfWork.Basic;
using Scalar.AspNetCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// appsettings.json から接続文字列を取得
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ===== データベース初期化 =====
DatabaseInitializer.Initialize(connectionString);

// DI登録
// Controller + 自前の ValidationFilter
builder.Services.AddControllers(options =>
{
    // グローバルに ValidationFilter を適用
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddOpenApi();

// FluentValidation（Validator のみ登録）
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Database
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");

    var conn = new SqliteConnection(connectionString);
    conn.Open();
    return conn;
});

// unit of work
builder.Services.AddScoped<IDbSessionAccessor, DbSessionAccessor>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ミドルウェア（例外ハンドリング用）
app.UseMiddleware<ProblemDetailsMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
