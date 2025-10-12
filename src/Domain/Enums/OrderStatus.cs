// ReSharper disable All

namespace CleanArchitectureBase.Domain.Enums;

public enum OrderStatus
{
    AWAITING_PAYMENT, // nếu đơn hàng là thanh toán online
    PENDING, // Nếu đơn hàng đang chờ phía admin xác nhận 
    PROCESSING, // Đang xử lý, đóng gói
    SHIPPED, // Đã chuyển đi
    DELIVERED, // Bên vận chuyển đã báo đã tới nơi
    CANCELLED, // Đã hủy trước khi admin xác nhận
    REFUNDING,
    REFUNDED
}
