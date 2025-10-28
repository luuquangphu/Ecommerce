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

            // Nếu có ảnh, lưu vào bảng FoodImages
            if (model.FoodImages != null && model.FoodImages.Any())
            {
                foreach (var img in model.FoodImages)
                    img.MenuId = model.MenuId;

                await db.FoodImages.AddRangeAsync(model.FoodImages);
                await db.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var menu = await db.Menus
                .Include(m => m.FoodImages)
                .FirstOrDefaultAsync(m => m.MenuId == id);

            if (menu != null)
            {
                // Xóa ảnh
                if (menu.FoodImages != null && menu.FoodImages.Any())
                    db.FoodImages.RemoveRange(menu.FoodImages);

                db.Menus.Remove(menu);
                await db.SaveChangesAsync();
            }
        }

        // ✅ GetAll: thêm cả danh sách ảnh (ImageUrls) để hiển thị trong Edit
        public async Task<IEnumerable<MenuViewModel>> GetAll(string? search = null)
        {
            var query = db.Menus
                .Include(m => m.MenuCategory)
                .Include(m => m.FoodImages)
                .Select(p => new MenuViewModel
                {
                    MenuId = p.MenuId,
                    MenuName = p.MenuName,
                    MenuCategoryName = p.MenuCategory.MenuCategoryName,
                    Detail = p.Detail,
                    MainImageUrl = p.FoodImages
                        .Where(f => f.MainImage)
                        .Select(f => f.UrlImage)
                        .FirstOrDefault(),
                    ImageUrls = p.FoodImages
                        .Select(f => f.UrlImage)
                        .ToList()
                });

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(x =>
                    x.MenuName.ToLower().Contains(search) ||
                    x.MenuCategoryName.ToLower().Contains(search) ||
                    (x.Detail != null && x.Detail.ToLower().Contains(search)));
            }

            return await query.ToListAsync();
        }

        // ✅ GetById: giữ nguyên — đã có MainImageUrl + ImageUrls
        public async Task<MenuViewModel> GetById(int id)
        {
            var menu = await db.Menus
                .Include(p => p.MenuCategory)
                .Include(p => p.FoodImages)
                .Select(p => new MenuViewModel
                {
                    MenuId = p.MenuId,
                    MenuName = p.MenuName,
                    MenuCategoryName = p.MenuCategory.MenuCategoryName,
                    Detail = p.Detail,
                    MainImageUrl = p.FoodImages
                        .Where(f => f.MainImage)
                        .Select(f => f.UrlImage)
                        .FirstOrDefault(),
                    ImageUrls = p.FoodImages
                        .Select(f => f.UrlImage)
                        .ToList()
                })
                .FirstOrDefaultAsync(m => m.MenuId == id);

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

        public async Task<Menu?> GetEntityById(int id)
        {
            return await db.Menus
                .Include(m => m.FoodImages)
                .FirstOrDefaultAsync(m => m.MenuId == id);
        }
    }
}
    