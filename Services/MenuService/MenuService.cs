using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Repositories.MenuRepository;
using Ecommerce.ViewModels;

namespace Ecommerce.Services.MenuService
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository menuRepository;

        public MenuService(IMenuRepository menuRepository)
        {
            this.menuRepository = menuRepository;
        }

        public async Task<StatusDTO> Create(Menu model)
        {
            var checkName = await menuRepository.ValidName(model.MenuName);
            if(checkName != null) 
                return new StatusDTO { IsSuccess = false, Message = "Tên món bị trùng" };
            await menuRepository.Create(model);
            return new StatusDTO { IsSuccess = true, Message = $"Tạo món {model.MenuName} thành công" };
        }

        public Task<StatusDTO> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MenuViewModel>> GetAll()
        {
            var lstMenu = menuRepository.GetAll();
            return lstMenu;
        }

        public Task<MenuViewModel> GetById(int id)
        {
            var menu = menuRepository.GetById(id);
            return menu;
        }

        public Task<StatusDTO> Update(Menu model)
        {
            throw new NotImplementedException();
        }
    }
}
