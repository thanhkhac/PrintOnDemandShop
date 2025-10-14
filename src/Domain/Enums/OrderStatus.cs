// ReSharper disable All

namespace CleanArchitectureBase.Domain.Enums;

/// <summary>
/// Admin:
/// Cho phép chọn qua lại các status sau: REJECTED, PROCESSING, SHIPPED
/// Không cho phép chỉnh 
/// </summary>
public enum OrderStatus
{
    PENDING,          // Thực hiện bởi người dùng | Nếu đơn hàng đang chờ phía admin xác nhận 
    REJECTED,         // Thực hiện bởi admin | Admin từ chối đơn hàng
    ACCEPTED,         // Thực hiện bởi admin | Xác thực
    PROCESSING,       // Thực hiện bởi admin | Đang xử lý, đóng gói
    SHIPPED,          // Thực hiện bởi admin 
    CONFIRM_RECEIVED, // Thực hiện bởi người dùng |  User xác nhận đã nhận hàng 
    CANCELLED,        // Thực hiện bởi người dùng | Cho phép hủy sau khi trước khi admin xác nhận | 
    EXPIRED,          // Thực hiện bởi hệ thống | Hết hạn thanh toán (tự động hủy sau 5 phút)
    RETURNED
}

public enum OrderPaymentStatus
{
    ONLINE_PAYMENT_AWAITING, 
    ONLINE_PAYMENT_PAID, // Nếu đơn hàng bị admin reject thì sẽ chuyển thành refunding
    COD,
    REFUNDING,
    REFUNDED, 
    EXPIRED // Hết hạn thanh toán
}
