using Ecommerce.Models;
using Ecommerce.ViewModels;
using static Ecommerce.DTO.MenuOrderDTO;

namespace Ecommerce.Repositories.MenuRepository
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuViewModel>> GetAll(string? search = null);

        Task<MenuViewModel> GetById(int id);
        Task Create(Menu model);
        Task Update(Menu model);
        Task Delete(int id);

        Task<string> ValidName(string name);
        Task<List<MenuDto>> GetAvailableMenusAsync();
    }
}
