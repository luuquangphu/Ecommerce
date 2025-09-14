using Ecommerce.Models;
using Ecommerce.ViewModels;
using static System.Net.WebRequestMethods;

namespace Ecommerce.Repositories.Account
{
    public interface IAccountRepository
    {
        Task<(bool isSuccess, string Message)> CreateAsync(RegisterViewModel customer, int rankId, string urlImage);
    }
}
