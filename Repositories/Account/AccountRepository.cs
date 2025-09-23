using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Services.JWT;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ecommerce.Repositories.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;


        public AccountRepository(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<(bool isSuccess, string Message)> CreateAccountAsync(RegisterViewModel model, int rankId, string urlImage)
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
            {
                var errors = result.Errors.Select(e => e.Description);
                return (false, "Lỗi tạo tài khoản");
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            await userManager.AddToRoleAsync(user, "User");
            return (true, "Tạo tài khoản thành công");
        }

        public async Task<(string userName, string role)> GetUserandRole(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return ("", "");

            //Phương thức này sẽ trả về các role của 1 user
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            if (role == null) return (user.Name, "");
            return (user.Name, role);
        }

        public async Task<Users> FindUserByEmail(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return user;
        }
    }
}
