using Ecommerce.Models;
using Ecommerce.ViewModels;
using static System.Net.WebRequestMethods;

namespace Ecommerce.Repositories.Account
{
    public interface IAccountRepository
    {
        Task<(bool isSuccess, string Message)> CreateAccountAsync(RegisterViewModel customer, int rankId, string urlImage);
        Task<(string userName, string role)> GetUserandRole(string email);
        Task<Users> FindUserByEmail(string email);
    }
}
