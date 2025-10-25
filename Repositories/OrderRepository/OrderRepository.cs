using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.OrderRepository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext db;

        public OrderRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<Order?> GetById(int orderId) => await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

        //Hàm để hiện đơn hàng phía nhà hàng
        public async Task<IEnumerable<Order>> GetAllOrderAsync()
        {
            return await db.Orders
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .Where(o => o.OrderStatus != "Hoàn thành")
                .OrderBy(o => o.OrderDate)
                .ToListAsync();
        }

        //Hiện thông tin chi tiết của 1 đơn phía nhà hàng
        public async Task<Order?> GetOrderDetailsByIdAsync(int orderId)
        {
            return await db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.FoodSize)
                        .ThenInclude(fs => fs.Menu)
                .Include(o => o.Table)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        //Hàm cập nhật OrderStatus(Trạng thái đơn hàng) phía bếp
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return false;

            order.OrderStatus = newStatus;

            await db.SaveChangesAsync();
            return true;
        }

        //Hàm yêu cầu thanh toán
        public async Task<bool> RequestPaymentAsync(int orderId)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return false;
            
            order.PaymentStatus = "Yêu cầu thanh toán";
            await db.SaveChangesAsync();
            return true;

        }


        //Hàm để người dùng lấy danh sách đơn hàng cùng chi tiết đơn của họ 
        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string customerId)
        {
            return await db.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.FoodSize)
                        .ThenInclude(fs => fs.Menu)
                .Where(o => o.CustomerId == customerId && o.PaymentStatus == "Yêu cầu thanh toán" || o.PaymentStatus == "Yêu cầu thanh toán")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

    }
}
