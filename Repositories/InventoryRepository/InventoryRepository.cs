using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.InventoryRepository
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext db;

        public InventoryRepository(AppDbContext db)
        {
            this.db = db;
        }

        // 🟢 Lấy toàn bộ danh sách tồn kho (kèm MenuName và FoodSizeName)
        public async Task<IEnumerable<InventoryViewModel>> GetAll()
        {
            var list = await db.Inventories
                .Include(i => i.FoodSize)
                    .ThenInclude(fs => fs.Menu) // đi qua FoodSize -> Menu
                .Select(i => new InventoryViewModel
                {
                    InventoryId = i.InventoryId,
                    MenuName = i.FoodSize.Menu.MenuName, // lấy từ Menu
                    FoodSizeName = i.FoodSize.FoodName,
                    Unit = i.Unit,
                    Quantity = i.Quantity
                })
                .ToListAsync();

            return list;
        }

        // 🟢 Lấy tồn kho theo ID
        public async Task<InventoryViewModel> GetById(int id)
        {
            var inventory = await db.Inventories
                .Include(i => i.FoodSize)
                    .ThenInclude(fs => fs.Menu)
                .Select(i => new InventoryViewModel
                {
                    InventoryId = i.InventoryId,
                    MenuName = i.FoodSize.Menu.MenuName,
                    FoodSizeName = i.FoodSize.FoodName,
                    Unit = i.Unit,
                    Quantity = i.Quantity
                })
                .FirstOrDefaultAsync(i => i.InventoryId == id);

            return inventory;
        }

        // 🟢 Tạo mới tồn kho
        public async Task Create(Inventory model)
        {
            await db.Inventories.AddAsync(model);
            await db.SaveChangesAsync();
        }

        // 🟢 Cập nhật tồn kho
        public async Task Update(Inventory model)
        {
            db.Inventories.Update(model);
            await db.SaveChangesAsync();
        }

        // 🟢 Xóa tồn kho
        public async Task Delete(int id)
        {
            var inventory = await db.Inventories.FindAsync(id);
            if (inventory != null)
            {
                db.Inventories.Remove(inventory);
                await db.SaveChangesAsync();
            }
        }
    }
}
