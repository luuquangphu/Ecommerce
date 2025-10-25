using Ecommerce.Data;
using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Repositories.CartRepository;
using Ecommerce.Repositories.OrderRepository;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly AppDbContext db;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, AppDbContext db)
        {
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.db = db;
        }

        public async Task<StatusDTO> CreateOrderAsync(string userId)
        {
            var cart = await cartRepository.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy giỏ hàng." };

            using var transaction = await db.Database.BeginTransactionAsync();
            var messages = new List<string>();

            try
            {
                foreach (var item in cart.CartDetails)
                {
                    // Lấy Inventory theo từng FoodSize
                    var inventory = await db.Inventories
                        .Include(i => i.FoodSize)
                        .ThenInclude(fs => fs.Menu)
                        .FirstOrDefaultAsync(i => i.FoodSizeId == item.FoodSizeId);

                    if (inventory == null)
                    {
                        messages.Add($"Không tìm thấy tồn kho cho món {item.FoodSize?.Menu?.MenuName ?? "không xác định"}.");
                        continue;
                    }

                    if (inventory.Quantity <= 0)
                    {
                        item.Count = 0;
                        messages.Add($"Món {inventory.FoodSize.Menu.MenuName} ({inventory.FoodSize.FoodName}) đã hết hàng và bị loại khỏi đơn.");
                        continue;
                    }

                    if (inventory.Quantity < item.Count)
                    {
                        messages.Add($"Món {inventory.FoodSize.Menu.MenuName} ({inventory.FoodSize.FoodName}) chỉ còn {inventory.Quantity} phần, đã giảm từ {item.Count} → {inventory.Quantity}.");
                        item.Count = inventory.Quantity;
                    }

                    // Trừ tồn kho thực tế
                    inventory.Quantity -= item.Count;
                    db.Inventories.Update(inventory);
                }

                cart.CartStatus = "Xác nhận";
                db.Carts.Update(cart);

                // Tạo đơn hàng
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = userId,
                    TotalAmount = cart.CartDetails.Sum(i => i.Count * i.FoodSize.Price),
                    OrderStatus = "Tiếp nhận đơn",
                    PaymentStatus = "Chưa thanh toán"
                };
                await db.Orders.AddAsync(order);

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                var message = messages.Any()
                    ? string.Join("\n", messages)
                    : "Giỏ hàng đã được xác nhận và trừ kho thành công.";

                return new StatusDTO { IsSuccess = true, Message = message };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new StatusDTO { IsSuccess = false, Message = "Lỗi khi xác nhận giỏ hàng: " + ex.Message };
            }
        }

        // 🔹 Lấy danh sách tất cả đơn hàng (phía nhà hàng)
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await orderRepository.GetAllOrderAsync();
        }

        // 🔹 Lấy thông tin chi tiết 1 đơn hàng
        public async Task<Order?> GetOrderDetailsByIdAsync(int orderId)
        {
            return await orderRepository.GetOrderDetailsByIdAsync(orderId);
        }

        // 🔹 Lấy danh sách đơn hàng của người dùng
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string customerId)
        {
            return await orderRepository.GetOrdersByUserIdAsync(customerId);
        }

        public async Task<StatusDTO> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var allowedStatuses = new[] { "Tiếp nhận đơn", "Đang chuẩn bị", "Hoàn thành" };

            if (!allowedStatuses.Contains(newStatus))
                return new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Trạng thái không hợp lệ. Chỉ được phép: Tiếp nhận đơn, Đang chuẩn bị, Hoàn thành."
                };

            var order = await orderRepository.GetById(orderId);
            if (order == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

            // Ngăn thay đổi trạng thái nếu đơn hàng đã hoàn thành
            if (order.OrderStatus == "Hoàn thành" && newStatus != "Hoàn thành")
                return new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Đơn hàng đã hoàn thành, không thể thay đổi trạng thái."
                };

            var success = await orderRepository.UpdateOrderStatusAsync(orderId, newStatus);

            return new StatusDTO
            {
                IsSuccess = success,
                Message = success
                    ? $"Cập nhật trạng thái đơn hàng sang '{newStatus}' thành công."
                    : "Cập nhật trạng thái thất bại."
            };
        }

        // 🔹 Gửi yêu cầu thanh toán (chỉ khi OrderStatus = Hoàn thành)
        public async Task<StatusDTO> RequestPaymentAsync(int orderId)
        {
            var order = await orderRepository.GetById(orderId);
            if (order == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy đơn hàng." };

            if (order.OrderStatus != "Hoàn thành")
                return new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Chỉ có thể yêu cầu thanh toán khi đơn hàng đã hoàn thành."
                };

            var success = await orderRepository.RequestPaymentAsync(orderId);

            return new StatusDTO
            {
                IsSuccess = success,
                Message = success
                    ? "Đã gửi yêu cầu thanh toán thành công."
                    : "Không thể gửi yêu cầu thanh toán."
            };
        }
    }
}
