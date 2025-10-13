// ReSharper disable All

namespace CleanArchitectureBase.Domain.Enums;

public enum OrderStatus
{
    PENDING, // Nếu đơn hàng đang chờ phía admin xác nhận 
    REJECTED, // Admin từ chối đơn hàng
    PROCESSING, // Đang xử lý, đóng gói
    SHIPPED, // Đã chuyển đi
    DELIVERED, // Bên vận chuyển đã báo đã tới nơi
    CONFIRM_RECEIVED, // User xác nhận đã nhận hàng
    CANCELLED, // Đã hủy trước khi admin xác nhận
}

public enum OrderPaymentStatus
{
    ONLINE_PAYMENT_AWAITING,
    ONLINE_PAYMENT_PAID,
    COD,
    REFUNDING,
    REFUNDED
}
