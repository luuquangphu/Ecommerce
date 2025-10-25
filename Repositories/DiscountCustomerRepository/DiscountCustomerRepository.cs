using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.DiscountCustomerRepository
{
    public class DiscountCustomerRepository : IDiscountCustomerRepository
    {
        private readonly AppDbContext context;

        public DiscountCustomerRepository(AppDbContext context)
        {
            this.context = context;
        }

        // ✅ Giữ nguyên code cũ của bạn
        public async Task<bool> CheckCustomerHasDiscount(string discountId, string customerId)
        {
            return await context.Discount_Customers
                .AnyAsync(x => x.DiscountId == discountId && x.CustomerId == customerId);
        }

        public async Task<bool> AddDiscountToCustomer(string discountId, string customerId)
        {
            var exists = await CheckCustomerHasDiscount(discountId, customerId);
            if (exists) return false;

            var dc = new Discount_Customer
            {
                DiscountId = discountId,
                CustomerId = customerId,
                isUsed = false
            };

            context.Discount_Customers.Add(dc);
            await context.SaveChangesAsync();
            return true;
        }

        // 🔹 Lấy danh sách discount theo CustomerId
        public async Task<IEnumerable<Discount_Customer>> GetByCustomerIdAsync(string customerId)
        {
            return await context.Discount_Customers
                .Include(dc => dc.Discount)
                .Where(dc => dc.CustomerId == customerId)
                .ToListAsync();
        }

        // 🔹 Lấy bản ghi cụ thể (1 mã của 1 user)
        public async Task<Discount_Customer?> GetByCustomerAndDiscountAsync(string customerId, string discountId)
        {
            return await context.Discount_Customers
                .FirstOrDefaultAsync(dc => dc.CustomerId == customerId && dc.DiscountId == discountId);
        }

        // 🔹 Cập nhật trạng thái sử dụng
        public async Task UpdateAsync(Discount_Customer entity)
        {
            context.Discount_Customers.Update(entity);
            await context.SaveChangesAsync();
        }
    }
}
