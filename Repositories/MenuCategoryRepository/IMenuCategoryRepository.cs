using Ecommerce.DTO;
using Ecommerce.Models;

namespace Ecommerce.Repositories.MenuCategoryRepository
{
    public interface IMenuCategoryRepository
    {
        Task<MenuCategory> GetByIdAsync(string id);
        Task<IEnumerable<MenuCategory>> GetAllAsync();
        Task<MenuCategory> CreateAsync(MenuCategory model);
        Task<bool> UpdateAsync(MenuCategory model);
        Task<bool> DeleteAsync(string id);

        Task<bool> HasMenuInCategoryAsync(string categoryId);
    }
}
