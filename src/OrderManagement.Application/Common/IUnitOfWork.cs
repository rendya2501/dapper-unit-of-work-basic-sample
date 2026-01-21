using OrderManagement.Application.Repositories;

namespace OrderManagement.Application.Common;

/// <summary>
/// Unit of Work パターンの中核インターフェース
/// </summary>
/// <remarks>
/// <para><strong>読み取り専用操作とトランザクション</strong></para>
/// <para>
/// 読み取り専用の操作（SELECT）では BeginTransaction() を呼ばなくても使用可能。
/// 更新操作（INSERT/UPDATE/DELETE）では必ず BeginTransaction() → Commit() を呼ぶ。
/// </para>
/// <code>
/// // 読み取り専用
/// using var uow = unitOfWorkFactory();
/// var items = await uow.Inventory.GetAllAsync(); // OK
/// 
/// // 更新操作
/// using var uow = unitOfWorkFactory();
/// uow.BeginTransaction();
/// await uow.Inventory.CreateAsync(item);
/// uow.Commit();
/// </code>
/// </remarks>
public interface IUnitOfWork : IDisposable
{
    #region トランザクション
    /// <summary>
    /// トランザクションを開始します
    /// </summary>
    /// <exception cref="InvalidOperationException">既にトランザクションが開始されている場合</exception>
    void BeginTransaction();

    /// <summary>
    /// トランザクションをコミットします
    /// </summary>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合</exception>
    void Commit();

    /// <summary>
    /// トランザクションをロールバックします
    /// </summary>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合</exception>
    void Rollback();
    #endregion
}
