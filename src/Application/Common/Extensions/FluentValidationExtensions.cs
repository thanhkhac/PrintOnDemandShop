using System.Collections;

namespace CleanArchitectureBase.Application.Common.Extensions;

public static class FluentValidationExtensions
{
    //TODO: Vẫn chưa chắc chắn, cần kiểm tra thử thêm
    /// <summary>
    /// Cho phép null, nhưng nếu có giá trị thì không được rỗng hoặc mặc định.
    /// Hỗ trợ: string, Guid, Guid?, int, int?, DateTime, DateTime?, object.
    /// </summary>
    public static IRuleBuilderOptions<T, TProperty> NullOrNotEmpty<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.Must(value =>
            {
                // Null thì hợp lệ
                if (value == null) return true;

                switch (value)
                {
                    case string s:
                        return !string.IsNullOrWhiteSpace(s);

                    case Guid g:
                        return g != Guid.Empty;
               
                    case IEnumerable enumerable:
                        return enumerable.Cast<object>().Any();

                    default:
                        // Các kiểu value type khác: cho phép mọi giá trị (kể cả default)
                        return true;
                }
            })
            .WithMessage("{PropertyName} must be null or not empty/default value.");
    }
}
