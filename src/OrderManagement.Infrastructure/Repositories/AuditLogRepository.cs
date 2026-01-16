using Dapper;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Repositories.Abstractions;
using System.Data;

namespace OrderManagement.Infrastructure.Repositories;

/// <summary>
/// 監査ログリポジトリの実装
/// </summary>
/// <param name="connection">データベース接続</param>
/// <param name="transaction">トランザクション（UnitOfWork から注入）</param>
public class AuditLogRepository(IDbConnection connection, IDbTransaction? transaction)
    : IAuditLogRepository
{
    /// <inheritdoc />
    public async Task CreateAsync(AuditLog log)
    {
        const string sql = """
            INSERT INTO AuditLog (Action, Details, CreatedAt)
            VALUES (@Action, @Details, @CreatedAt)
            """;

        await connection.ExecuteAsync(sql, log, transaction);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditLog>> GetAllAsync(int limit = 100)
    {
        const string sql = """
            SELECT * FROM AuditLog 
            ORDER BY CreatedAt DESC 
            LIMIT @Limit
            """;

        return await connection.QueryAsync<AuditLog>(
            sql, new { Limit = limit }, transaction);
    }
}
