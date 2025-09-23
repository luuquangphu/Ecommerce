using Ecommerce.Models;
using Ecommerce.ViewModels;

namespace Ecommerce.Services.JWT
{
    public interface IJwtService
    {
        Task<string> CreateToken(Users user);
    }
}
