using Ecommerce.DTO;
using Ecommerce.Models;

namespace Ecommerce.Services.OrderService
{
    public interface IOrderService
    {
        Task<StatusDTO> CreateOrderAsync(string userId);
        Task<Order?> GetOrderDetailsByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string customerId);
        Task<StatusDTO> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<StatusDTO> RequestPaymentAsync(int orderId);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
    }
}
