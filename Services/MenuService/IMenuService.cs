using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.ViewModels;

namespace Ecommerce.Services.MenuService
{
    public interface IMenuService
    {
        Task<IEnumerable<MenuViewModel>> GetAll();
        Task<MenuViewModel> GetById(int id);
        Task<StatusDTO> Create(Menu model);
        Task<StatusDTO> Update(Menu model);
        Task<StatusDTO> Delete(int id);
    }
}
