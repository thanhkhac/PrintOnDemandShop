// ReSharper disable InconsistentNaming

namespace CleanArchitectureBase.Domain.Constants;

public static class ErrorCodes
{
    //COMMON
    public const string COMMON_FORBIDDEN = nameof(COMMON_FORBIDDEN);
    public const string COMMON_INVALID_MODEL = nameof(COMMON_INVALID_MODEL);
    public const string COMMON_UNAUTHORIZED = nameof(COMMON_UNAUTHORIZED);
    public const string COMMON_SERVER_INTERNAL_ERROR = nameof(COMMON_SERVER_INTERNAL_ERROR);
    public const string COMMON_NOT_FOUND = nameof(COMMON_NOT_FOUND);
    public const string FIELD_NAME_NOT_FOUND = nameof(FIELD_NAME_NOT_FOUND);
    public const string COMMON_INVALID_REQUEST = nameof(COMMON_INVALID_REQUEST);

    //ACCOUNT
    public const string ACCOUNT_NOTFOUND = nameof(ACCOUNT_NOTFOUND);
    public const string ACCOUNT_LOCKED_OUT = nameof(ACCOUNT_LOCKED_OUT);
    public const string ACCOUNT_BANNED = nameof(ACCOUNT_BANNED);
    public const string ACCOUNT_INVALID_CREDENTIALS = nameof(ACCOUNT_INVALID_CREDENTIALS);
    public const string ACCOUNT_INVALID_VERIFICATION_CODE = nameof(ACCOUNT_INVALID_VERIFICATION_CODE);
    
    public const string ACCOUNT_EMAIL_NOT_VERIFIED = nameof(ACCOUNT_EMAIL_NOT_VERIFIED);
    
    public const string ACCOUNT_EMAIL_BANNED = nameof(ACCOUNT_EMAIL_BANNED);
    public const string ACCOUNT_INVALID_RESET_CODE = nameof(ACCOUNT_INVALID_RESET_CODE);
    public const string ACCOUNT_WRONG_PASSWORD = nameof(ACCOUNT_WRONG_PASSWORD);
    public const string EMAIL_VERIFICATION_REQUEST_TOO_MANY = nameof(EMAIL_VERIFICATION_REQUEST_TOO_MANY);
    public const string EMAIL_VERIFICATION_CODE_FAILED_TOO_MANY = nameof(EMAIL_VERIFICATION_CODE_FAILED_TOO_MANY);
    public const string PASSWORD_RESET_REQUEST_TOO_MANY = nameof(PASSWORD_RESET_REQUEST_TOO_MANY);
    public const string PASSWORD_RESET_CODE_FAILED_TOO_MANY = nameof(PASSWORD_RESET_CODE_FAILED_TOO_MANY);
    public const string REFRESHTOKEN_NOTFOUND = nameof(REFRESHTOKEN_NOTFOUND);


    //ROLE
    public const string ROLE_NOTFOUND = nameof(ROLE_NOTFOUND);

    //USER
    public const string USER_NOTFOUND = nameof(USER_NOTFOUND);

    //PAYMENT
    public const string PAYMENT_TRANSACTION_EXISTED = nameof(PAYMENT_TRANSACTION_EXISTED);


    //IDENTITY OVERRIDE ERROR DESCRIBER
    public const string IDENTITY_DEFAULT_ERROR = nameof(IDENTITY_DEFAULT_ERROR);
    public const string IDENTITY_CONCURRENCY_FAILURE = nameof(IDENTITY_CONCURRENCY_FAILURE);
    public const string IDENTITY_PASSWORD_MISMATCH = nameof(IDENTITY_PASSWORD_MISMATCH);
    public const string IDENTITY_INVALID_TOKEN = nameof(IDENTITY_INVALID_TOKEN);
    public const string IDENTITY_RECOVERY_CODE_REDEMPTION_FAILED = nameof(IDENTITY_RECOVERY_CODE_REDEMPTION_FAILED);
    public const string IDENTITY_LOGIN_ALREADY_ASSOCIATED = nameof(IDENTITY_LOGIN_ALREADY_ASSOCIATED);
    public const string IDENTITY_INVALID_USER_NAME = nameof(IDENTITY_INVALID_USER_NAME);
    public const string IDENTITY_INVALID_EMAIL = nameof(IDENTITY_INVALID_EMAIL);
    public const string IDENTITY_DUPLICATE_USER_NAME = nameof(IDENTITY_DUPLICATE_USER_NAME);
    public const string IDENTITY_DUPLICATE_EMAIL = nameof(IDENTITY_DUPLICATE_EMAIL);
    public const string IDENTITY_INVALID_ROLE_NAME = nameof(IDENTITY_INVALID_ROLE_NAME);
    public const string IDENTITY_DUPLICATE_ROLE_NAME = nameof(IDENTITY_DUPLICATE_ROLE_NAME);
    public const string IDENTITY_USER_ALREADY_HAS_PASSWORD = nameof(IDENTITY_USER_ALREADY_HAS_PASSWORD);
    public const string IDENTITY_USER_LOCKOUT_NOT_ENABLED = nameof(IDENTITY_USER_LOCKOUT_NOT_ENABLED);
    public const string IDENTITY_USER_ALREADY_IN_ROLE = nameof(IDENTITY_USER_ALREADY_IN_ROLE);
    public const string IDENTITY_USER_NOT_IN_ROLE = nameof(IDENTITY_USER_NOT_IN_ROLE);
    public const string IDENTITY_PASSWORD_TOO_SHORT = nameof(IDENTITY_PASSWORD_TOO_SHORT);
    public const string IDENTITY_PASSWORD_REQUIRES_UNIQUE_CHARS = nameof(IDENTITY_PASSWORD_REQUIRES_UNIQUE_CHARS);
    public const string IDENTITY_PASSWORD_REQUIRES_NON_ALPHANUMERIC = nameof(IDENTITY_PASSWORD_REQUIRES_NON_ALPHANUMERIC);
    public const string IDENTITY_PASSWORD_REQUIRES_DIGIT = nameof(IDENTITY_PASSWORD_REQUIRES_DIGIT);
    public const string IDENTITY_PASSWORD_REQUIRES_LOWER = nameof(IDENTITY_PASSWORD_REQUIRES_LOWER);
    public const string IDENTITY_PASSWORD_REQUIRES_UPPER = nameof(IDENTITY_PASSWORD_REQUIRES_UPPER);


    public const string CATEGORY_NOT_FOUND = nameof(CATEGORY_NOT_FOUND);


    public const string PRODUCT_NOT_FOUND = nameof(PRODUCT_NOT_FOUND);


    public const string PRODUCT_VARIANT_NOT_FOUND = nameof(PRODUCT_VARIANT_NOT_FOUND);
    public const string INSUFFICIENT_STOCK = nameof(INSUFFICIENT_STOCK);


    public const string PRODUCT_DESIGN_NOT_FOUND = nameof(PRODUCT_DESIGN_NOT_FOUND);

    public const string CART_ITEM_NOT_FOUND = nameof(CART_ITEM_NOT_FOUND);

    public const string VOUCHER_NOT_FOUND = nameof(VOUCHER_NOT_FOUND);
    public const string VOUCHER_INVALID_DATE = nameof(VOUCHER_INVALID_DATE);


    /// <summary>
    /// Cho API POST /api/Order
    /// Data sẽ là list ids của các variant không tồn tại
    /// Có có thể lấy các id đó để xử lý danh sách các sản phẩm trong order (nếu muốn)
    /// </summary>
    public const string ORDER_VARIANTS_NOT_FOUND = nameof(ORDER_VARIANTS_NOT_FOUND);

    public const string ORDER_NOT_FOUND = nameof(ORDER_NOT_FOUND);
    public const string ORDER_IS_NOT_AWAITING_ONLINE_PAYMENT = nameof(ORDER_IS_NOT_AWAITING_ONLINE_PAYMENT);
    public const string ORDER_INSUFFICIENT_PAYMENT_AMOUNT = nameof(ORDER_INSUFFICIENT_PAYMENT_AMOUNT);
    
    
    public const string TOKEN_PACKAGE_NOT_FOUND = nameof(TOKEN_PACKAGE_NOT_FOUND);
}
