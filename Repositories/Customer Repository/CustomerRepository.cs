
using Ecommerce.Data;
using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Services.Vaild;
using Ecommerce.ViewModels;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Ecommerce.Repositories.CustomerRepository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext db;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IVaildService vaildService;

        public CustomerRepository(AppDbContext db, UserManager<Users> userManager, IVaildService vaildService, RoleManager<IdentityRole> roleManager) 
        {
            this.db = db;
            this.userManager = userManager;
            this.vaildService = vaildService;
            this.roleManager = roleManager;
        }

        public async Task<IdentityResult> CreateAsync(RegisterViewModel model, string urlImage, int rankId, string role)
        {
            var user = new Customer
            {
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                UrlImage = urlImage,
                Email = model.Email,
                RankId = rankId,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                UserName = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRoleAsync(user, role);

            return IdentityResult.Success;
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string PhoneNumber, string userId)
        {
            return await db.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == PhoneNumber && c.Id != userId);
        }

        public async Task<CustomerRank?> GetRankByPointAsync(int point)
        {
            return await db.CustomerRanks
                .Where(r => point >= r.RankPoint)
                .OrderByDescending(r => r.RankPoint)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsCustomerCart(string userId)
        {
            var hasOrders = await db.Carts.AnyAsync(c => c.CustomerId == userId);

            //Nếu có đơn hàng return true
            if (hasOrders) return true;
            return false;
        }

        public async Task<List<CustomerDTO>> GetAll(string? search = null)
        {
            var query = db.Customers
                .Include(c => c.CustomerRank)
                .AsQueryable();

            // Nếu có từ khoá search thì lọc theo Name hoặc Email
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Email.Contains(search));
            }

            var lstCustomer = await query
                .Select(c => new CustomerDTO
                {
                    CustomerId = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Gender = c.Gender,
                    DateOfBirth = c.DateOfBirth,
                    RankName = c.CustomerRank.RankName,
                    UrlImage = c.UrlImage,
                    Point = c.Point
                })
                .ToListAsync();

            return lstCustomer;
        }

        public async Task<Customer> GetById(string userId)
        {
            var userItem = await db.Customers.Include(c => c.CustomerRank).FirstOrDefaultAsync(c => c.Id == userId);
            return userItem;
        }

        public async Task<Customer> Update(UpdateCustomerViewModel model, string userId, IFormFile? UrlImage)
        {
            var customer = await db.Customers.FindAsync(userId);

            // Cập nhật thông tin khách hàng
            if (customer != null)
            {
                customer.Name = model.Name;
                customer.PhoneNumber = model.PhoneNumber;
                customer.Email = model.Email;
                customer.DateOfBirth = model.DateOfBirth;
                customer.Gender = model.Gender;
                customer.RankId = model.RankId;

                if (customer.Point != model.Point)
                {
                    var rank = await db.CustomerRanks
                        .Where(r => model.Point >= r.RankPoint)
                        .OrderByDescending(r => r.RankPoint)
                        .FirstOrDefaultAsync();

                    customer.RankId = (int)(rank?.RankId);
                    customer.Point = model.Point;
                }
            }

            // Cập nhật ảnh nếu có
            if (UrlImage != null)
            {
                var imagePath = await vaildService.SaveImage(UrlImage, model.Name, model.PhoneNumber, "");
                customer.UrlImage = imagePath;
            }

            await db.SaveChangesAsync();
            return customer;
        }

        public async Task<StatusDTO> Delete(Customer customer)
        {
            var checkCustomer = await GetById(customer.Id);
            if (checkCustomer == null)
            {
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy người dùng cần xóa" };
            }
            db.Customers.Remove(customer);
            await db.SaveChangesAsync();

            return new StatusDTO { IsSuccess = true, Message = "Xóa thành công" };
        }
    }
}
