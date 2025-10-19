using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.MenuRepository
{
    public class MenuRepository : IMenuRepository
    {
        private readonly AppDbContext db;

        public MenuRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task Create(Menu model)
        {
            await db.Menus.AddAsync(model);
            await db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var menu = await db.Menus.FindAsync(id);
            db.Menus.Remove(menu);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<MenuViewModel>> GetAll()
        {
            var lstMenu = await db.Menus
                .Include(m => m.MenuCategory)
                .Select(p => new MenuViewModel{
                    MenuId = p.MenuId,
                    MenuName = p.MenuName,
                    MenuCategoryName = p.MenuCategory.MenuCategoryName,
                    Detail = p.Detail,
            }).ToListAsync();

            return lstMenu;
        }

        public async Task<MenuViewModel> GetById(int id)
        {
            var menu = await db.Menus.Select(p => new MenuViewModel
            {
                MenuId = p.MenuId,
                MenuName = p.MenuName,
                MenuCategoryName = p.MenuCategory.MenuCategoryName,
                Detail = p.Detail,
            }).FirstOrDefaultAsync(m => m.MenuId == id);

            return menu;
        }

        public async Task Update(Menu model)
        {
            db.Menus.Update(model);
            await db.SaveChangesAsync();
        }

        public async Task<string> ValidName(string name)
        {
            var result = await db.Menus.FirstOrDefaultAsync(m => m.MenuName == name);
            if (result == null) return "";
            return "Tên món ăn bị trùng";
        }
    }
}
