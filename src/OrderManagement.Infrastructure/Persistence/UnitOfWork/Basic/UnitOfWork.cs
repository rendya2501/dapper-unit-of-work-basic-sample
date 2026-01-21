using OrderManagement.Application.Common;
using System.Data;

namespace OrderManagement.Infrastructure.Persistence.UnitOfWork.Basic;

/// <summary>
/// Unit of Work の実装クラス
/// </summary>
/// <remarks>
public class UnitOfWork(IDbSessionAccessor accessor) : IUnitOfWork
{
    private bool _disposed;


    #region トランザクション制御
    /// <inheritdoc />
    public void BeginTransaction()
    {
        if (accessor.Connection.State != ConnectionState.Open)
            accessor.Connection.Open();

        if (accessor.Transaction != null)
            throw new InvalidOperationException("Transaction is already started.");

        accessor.Transaction = accessor.Connection.BeginTransaction();
    }

    /// <inheritdoc />
    public void Commit()
    {
        if (accessor.Transaction == null)
            throw new InvalidOperationException("Transaction is not started.");

        try
        {
            accessor.Transaction.Commit();
        }
        finally
        {
            accessor.Transaction.Dispose();
            accessor.Transaction = null;
        }
    }

    //public async Task CommitAsync(CancellationToken ct = default)
    //{
    //    if (accessor.Transaction is DbTransaction dbTrans)
    //        await dbTrans.CommitAsync(ct);
    //    else
    //        accessor.Transaction?.Commit();

    //    // ...以降、Dispose処理
    //}

    /// <inheritdoc />
    public void Rollback()
    {
        if (accessor.Transaction == null)
            throw new InvalidOperationException("Transaction is not started.");

        try
        {
            accessor.Transaction.Rollback();
        }
        finally
        {
            accessor.Transaction.Dispose();
            accessor.Transaction = null;
        }
    }
    #endregion


    #region Dispose パターン
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソースを解放します
    /// </summary>
    /// <param name="disposing">マネージドリソースを解放する場合は true</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // もしコミットされずに Dispose された（＝エラーが発生した）場合、
            // 明示的にロールバックして接続状態をクリーンにする
            if (accessor.Transaction != null)
            {
                accessor.Transaction.Rollback();
                accessor.Transaction.Dispose();
                accessor.Transaction = null;
            }
            // Connection の Dispose は DI (Scoped) に任せるならここでは呼ばない
        }

        _disposed = true;
    }
    #endregion
}
