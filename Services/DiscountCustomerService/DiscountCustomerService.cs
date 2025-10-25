using Ecommerce.Models;
using Ecommerce.Repositories.DiscountCustomerRepository;

namespace Ecommerce.Services.DiscountCustomerService
{
    public class DiscountCustomerService : IDiscountCustomerService
    {
        private readonly IDiscountCustomerRepository _discountCustomerRepo;

        public DiscountCustomerService(IDiscountCustomerRepository discountCustomerRepo)
        {
            _discountCustomerRepo = discountCustomerRepo;
        }

        // ✅ Lấy danh sách mã giảm giá của người dùng
        public async Task<IEnumerable<object>> GetUserDiscountsAsync(string customerId)
        {
            var list = await _discountCustomerRepo.GetByCustomerIdAsync(customerId);

            return list.Select(dc => new
            {
                dc.DiscountId,
                dc.CustomerId,
                dc.isUsed,
                discount = new
                {
                    dc.Discount.DiscountName,
                    dc.Discount.DiscountCategory,
                    dc.Discount.DiscountPrice,
                    dc.Discount.DateStart,
                    dc.Discount.DateEnd,
                    dc.Discount.DiscountStatus
                }
            });
        }

        // ✅ Sử dụng mã giảm giá (đánh dấu isUsed = true)
        public async Task<bool> UseDiscountAsync(string customerId, string discountId)
        {
            var record = await _discountCustomerRepo.GetByCustomerAndDiscountAsync(customerId, discountId);
            if (record == null || record.isUsed)
                return false;

            record.isUsed = true;
            await _discountCustomerRepo.UpdateAsync(record);
            return true;
        }

        // ✅ Admin gán mã cho người dùng
        public async Task<bool> AssignDiscountToUserAsync(string discountId, string customerId)
        {
            return await _discountCustomerRepo.AddDiscountToCustomer(discountId, customerId);
        }
    }
}
