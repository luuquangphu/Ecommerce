using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.DTO.MenuOrderDTO;

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

        public async Task<IEnumerable<MenuViewModel>> GetAll(string? search = null)
        {
            var query = db.Menus
                .Include(m => m.MenuCategory)
                .Select(p => new MenuViewModel
                {
                    MenuId = p.MenuId,
                    MenuName = p.MenuName,
                    MenuCategoryName = p.MenuCategory.MenuCategoryName,
                    Detail = p.Detail
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

        public async Task<List<MenuDto>> GetAvailableMenusAsync()
        {
            var menus = await db.Menus
                .Select(m => new MenuDto
                {
                    MenuId = m.MenuId,
                    MenuName = m.MenuName,

                    // 🔸 Lấy ảnh (ưu tiên MainImage, fallback ảnh đầu tiên nếu MainImage hỏng)
                    Images = db.FoodImages
                        .Where(img => img.MenuId == m.MenuId)
                        .OrderByDescending(img => img.MainImage)
                        .ThenBy(img => img.SortOrder)
                        .Select(img => img.UrlImage)
                        .Where(url => url != null) 
                        .Select(url => url!)
                        .ToList(),

                    // 🔸 Lấy danh sách biến thể (chỉ lấy biến thể có tồn kho > 0)
                    Variants = db.FoodSizes
                        .Where(fs => fs.MenuId == m.MenuId &&
                                     db.Inventories.Any(inv => inv.FoodSizeId == fs.FoodSizeId && inv.Quantity > 0))
                        .Select(fs => new FoodSizeDto
                        {
                            FoodSizeId = fs.FoodSizeId,
                            FoodSizeName = fs.FoodName,
                            Price = fs.Price
                        }).ToList()
                })
                .ToListAsync();

            // 🔸 Lọc món có ít nhất 1 biến thể còn hàng
            menus = menus.Where(m => m.Variants.Any()).ToList();

            return menus;
        }
    }
}
