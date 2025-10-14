namespace CleanArchitectureBase.Application.Orders.Interfaces;

public interface IOrderService
{
    public Task RestockOrder(Guid orderId);
}

