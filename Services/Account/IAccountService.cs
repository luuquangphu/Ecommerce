using Ecommerce.ViewModels;
using System.Runtime.CompilerServices;

namespace Ecommerce.Services.Account
{
    public interface IAccountService
    {
        Task<(bool isSuccess, string Message)> RegisterAsync(RegisterViewModel model, IFormFile imageFile);
    }
}
