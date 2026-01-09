using FluentValidation;
using OrderManagement.Api.Requests;

namespace OrderManagement.Api.Validators;

/// <summary>
/// 注文作成リクエストのバリデーター
/// </summary>
/// <remarks>
/// <para><strong>inheritdoc とは</strong></para>
/// <para>
/// 親クラスやインターフェースのXMLドキュメントコメントを継承する機能。
/// 同じ内容を繰り返し書く必要がなくなる。
/// </para>
/// <code>
/// // インターフェース
/// /// &lt;summary&gt;注文を作成します&lt;/summary&gt;
/// Task CreateAsync();
/// 
/// // 実装クラス
/// /// &lt;inheritdoc /&gt;  ← 上記のコメントが自動的に継承される
/// public Task CreateAsync() { }
/// </code>
/// </remarks>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        // 顧客IDは1以上
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer ID must be greater than 0.");

        // アイテムは必須
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item.");

        // 各アイテムのバリデーション
        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}
