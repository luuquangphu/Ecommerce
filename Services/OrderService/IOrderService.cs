using Ecommerce.DTO;
using Ecommerce.Models;

namespace Ecommerce.Services.OrderService
{
    public interface IOrderService
    {
        Task<StatusDTO> CreateOrderAsync(string userId);
        Task<IEnumerable<OrderDetailDTO>?> GetOrderDetailsByIdAsync(int orderId);
        Task<IEnumerable<OrderUserDTO>> GetOrdersByUserIdAsync(string customerId);
        Task<StatusDTO> UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<StatusDTO> RequestPaymentAsync(int orderId);
        Task<OrderDTO> GetAllOrdersAsync();
    }
}
