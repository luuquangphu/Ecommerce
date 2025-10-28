using Ecommerce.Data;
using Ecommerce.DTO;
using Ecommerce.HubSocket;
using Ecommerce.Models;
using Ecommerce.Repositories.CartRepository;
using Ecommerce.Repositories.DiscountCustomerRepository;
using Ecommerce.Repositories.DiscountRepository;
using Ecommerce.Repositories.OrderRepository;
using Ecommerce.Repositories.RankAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Services.OrderService
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICartRepository cartRepository;
        private readonly AppDbContext db;
        private readonly IHubContext<OrderHub> hubContext;
        private readonly IDiscountRepository discountRepository;
        private readonly UserManager<Users> userManager;
        private readonly IRankAccountRepository rankAccountRepository;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository, AppDbContext db, IHubContext<OrderHub> hubContext,
            IDiscountRepository discountRepository, UserManager<Users> userManager, IRankAccountRepository rankAccountRepository)
        {
            this.orderRepository = orderRepository;
            this.cartRepository = cartRepository;
            this.db = db;
            this.hubContext = hubContext;
            this.discountRepository = discountRepository;
            this.userManager = userManager;
            this.rankAccountRepository = rankAccountRepository;
        }

        public async Task<StatusDTO> CreateOrderAsync(string userId)
        {
            var cart = await cartRepository.GetActiveCartByUserIdAsync(userId);
            if (cart == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy giỏ hàng." };

            var messages = new List<string>();
            bool hasError = false;

            // 🔹 Bước 1: Kiểm tra kho
            foreach (var item in cart.CartDetails)
            {
                var inventory = await db.Inventories
                    .Include(i => i.FoodSize)
                    .ThenInclude(fs => fs.Menu)
                    .FirstOrDefaultAsync(i => i.FoodSizeId == item.FoodSizeId);

                if (inventory == null)
                {
                    messages.Add($"Không tìm thấy tồn kho cho món {item.FoodSize?.Menu?.MenuName ?? "không xác định"}.");
                    item.Count = 0;
                    hasError = true;
                    continue;
                }

                if (inventory.Quantity <= 0)
                {
                    messages.Add($"Món {inventory.FoodSize.Menu.MenuName} ({inventory.FoodSize.FoodName}) đã hết hàng và bị loại khỏi đơn.");
                    item.Count = 0;
                    hasError = true;
                    continue;
                }

                if (inventory.Quantity < item.Count)
                {
                    messages.Add($"Món {inventory.FoodSize.Menu.MenuName} ({inventory.FoodSize.FoodName}) chỉ còn {inventory.Quantity} phần, đã giảm từ {item.Count} → {inventory.Quantity}.");
                    item.Count = inventory.Quantity;
                    hasError = true;
                }

                db.Cart_Menus.Update(item);
            }

            await db.SaveChangesAsync();

            if (hasError)
            {
                return new StatusDTO
                {
                    IsSuccess = false,
                    Message = string.Join("\n", messages) + "\nVui lòng kiểm tra lại giỏ hàng."
                };
            }

            // 🔹 Bước 2: Transaction
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 🔹 Giảm tồn kho
                foreach (var item in cart.CartDetails)
                {
                    var inventory = await db.Inventories.FirstOrDefaultAsync(i => i.FoodSizeId == item.FoodSizeId);
                    if (inventory != null)
                    {
                        inventory.Quantity -= item.Count;
                        db.Inventories.Update(inventory);
                    }
                }

                // 🔹 Lấy chi tiết giỏ hàng (CartDetails hoặc getCartItem)
                var cartDetails = await cartRepository.getCartItem(userId);

                // 🔹 Cập nhật giỏ hàng
                cart.CartStatus = "Xác nhận";
                db.Carts.Update(cart);

                // 🔹 Tạo Order
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    CustomerId = userId,
                    TableId = cart.TableId,
                    TotalAmount = cart.CartDetails.Sum(i => i.Count * i.FoodSize.Price),
                    OrderStatus = "Tiếp nhận đơn",
                    PaymentStatus = "Chưa thanh toán"
                };
                await db.Orders.AddAsync(order);
                await db.SaveChangesAsync();

                

                // 🔹 Tạo danh sách OrderDetails
                var orderDetailsList = new List<OrderDetails>();

                foreach (var item in cartDetails)
                {
                    var detail = new OrderDetails
                    {
                        OrderId = order.OrderId,
                        FoodSizeId = item.FoodSizeId,
                        Count = item.Count,
                    };
                    orderDetailsList.Add(detail);
                }

                await db.OrderDetails.AddRangeAsync(orderDetailsList);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();

                // 🔹 Gửi thông báo
                await hubContext.Clients.Groups("Admins", "Staffs").SendAsync("ReceiveNewOrder", new
                {
                    OrderId = order.OrderId,
                    TableName = cart.Table?.TableName ?? "Không xác định",
                    TotalAmount = order.TotalAmount
                });

                return new StatusDTO { IsSuccess = true, Message = "Giỏ hàng đã được xác nhận và tạo đơn hàng thành công." };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Lỗi khi xác nhận giỏ hàng: " + (ex.InnerException?.Message ?? ex.Message)
                };
            }
        }

        // 🔹 Lấy danh sách tất cả đơn hàng (phía nhà hàng)
        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            return await orderRepository.GetAllOrderAsync();
        }

        // 🔹 Lấy thông tin chi tiết 1 đơn hàng
        public async Task<IEnumerable<OrderDetailDTO>?> GetOrderDetailsByIdAsync(int orderId)
        {
            return await orderRepository.GetOrderDetailsByIdAsync(orderId);
        }

        // 🔹 Lấy danh sách đơn hàng của người dùng
        public async Task<IEnumerable<OrderUserDTO>> GetOrdersByUserIdAsync(string customerId)
        {
            return await orderRepository.GetOrdersByUserIdAsync(customerId);
        }

        //Cập nhật trạng thái đơn hàng
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

            if (success)
            {
                // 🔥 Gửi socket thông báo realtime cho khách hàng
                var customerId = order.CustomerId; // Lấy Id khách hàng của đơn
                await hubContext.Clients.Group($"User_{customerId}")
                    .SendAsync("OrderStatusChanged", new
                    {
                        OrderId = order.OrderId,
                        NewStatus = newStatus,
                        Message = $"Đơn hàng #{order.OrderId} đã chuyển sang trạng thái '{newStatus}'"
                    });
            }

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

        public async Task<StatusDTO> UpdatePaymentMethodandTotal(ConfirmPaymentDTO model)
        {
            var order = await orderRepository.GetById(model.OrderId);
            if (order == null)
                return new StatusDTO { IsSuccess = false, Message = "Đơn hàng không tồn tại" };

            order.PaymentMethod = model.PaymentMethod;
            order.PaymentStatus = "Chờ xác nhận thanh toán";

            string? discountCategory = null;
            decimal? discountPrice = null;

            // 🔹 Xử lý mã giảm giá nếu có
            if (!string.IsNullOrWhiteSpace(model.DiscountId))
            {
                var discount = await discountRepository.GetById(model.DiscountId);
                if (discount == null)
                    return new StatusDTO { IsSuccess = false, Message = "Mã giảm giá không tồn tại" };

                var discountCustomer = await discountRepository.GetDiscountByUserId(order.CustomerId, model.DiscountId);
                if (discountCustomer == null)
                    return new StatusDTO { IsSuccess = false, Message = "Người dùng không có mã giảm giá này" };

                discountCustomer.isUsed = true;

                // ✅ Lưu thông tin discount để tính
                discountCategory = discount.DiscountCategory;
                discountPrice = discount.DiscountPrice;
            }

            // 🔹 Lấy giảm giá theo hạng thành viên
            var rankDiscount = await rankAccountRepository.GetRankPointByUserIdAsync(order.CustomerId);

            // 🔹 Tính tổng tiền cuối cùng
            var total = CalculateTotal(order.TotalAmount, discountCategory, discountPrice, rankDiscount ?? 0);

            // 🔹 Cập nhật lại vào đơn hàng
            order.TotalAmount = total;

            await db.SaveChangesAsync();

            return new StatusDTO
            {
                IsSuccess = true,
                Message = "Cập nhật giá tiền và phương thức thanh toán thành công"
            };
        }


        private decimal CalculateTotal(decimal total, string? discountCategory, decimal? discountPrice, int? rankDiscount)
        {
            decimal totalDiscount = 0;
            decimal totalRankDiscount = 0;

            // Giảm giá theo mã khuyến mãi
            if (discountPrice != null && discountPrice > 0)
            {
                if (discountCategory == "Phần trăm")
                {
                    totalDiscount = total * ((decimal)discountPrice / 100);
                }
                else
                {
                    totalDiscount = (decimal)discountPrice;
                }
            }

            // Giảm giá theo hạng thành viên
            if (rankDiscount != null && rankDiscount > 0)
            {
                totalRankDiscount = total * ((decimal)rankDiscount / 100);
            }

            decimal finalTotal = total - totalDiscount - totalRankDiscount;
            if (finalTotal < 0) finalTotal = 0; // tránh âm tiền

            return finalTotal;
        }

    }
}
