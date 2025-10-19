using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.FoodSizeRepository
{
    public class FoodSizeRepository : IFoodSizeRepository
    {
        private readonly AppDbContext db;

        public FoodSizeRepository(AppDbContext db)
        {
            this.db = db;
        }

        // =====================================
        // GET ALL - Lấy tất cả kích cỡ món ăn
        // =====================================
        public async Task<IEnumerable<FoodSizeViewModel>> GetAll()
        {
            var lstFoodSize = await db.FoodSizes
                .Include(fs => fs.Menu)
                .Select(fs => new FoodSizeViewModel
                {
                    FoodSizeId = fs.FoodSizeId,
                    MenuName = fs.Menu.MenuName,
                    FoodName = fs.FoodName,
                    Price = fs.Price,
                    SortOrder = fs.SortOrder
                }).ToListAsync();

            return lstFoodSize;
        }

        // =====================================
        // GET BY ID
        // =====================================
        public async Task<FoodSizeViewModel> GetById(int id)
        {
            var foodSize = await db.FoodSizes
                .Include(fs => fs.Menu)
                .Select(fs => new FoodSizeViewModel
                {
                    FoodSizeId = fs.FoodSizeId,
                    MenuName = fs.Menu.MenuName,
                    FoodName = fs.FoodName,
                    Price = fs.Price,
                    SortOrder = fs.SortOrder
                })
                .FirstOrDefaultAsync(fs => fs.FoodSizeId == id);

            return foodSize;
        }

        // =====================================
        // CREATE
        // =====================================
        public async Task Create(FoodSize model)
        {
            await db.FoodSizes.AddAsync(model);
            await db.SaveChangesAsync();
        }

        // =====================================
        // UPDATE
        // =====================================
        public async Task Update(FoodSize model)
        {
            db.FoodSizes.Update(model);
            await db.SaveChangesAsync();
        }

        // =====================================
        // DELETE
        // =====================================
        public async Task Delete(int id)
        {
            var foodSize = await db.FoodSizes.FindAsync(id);
            if (foodSize != null)
            {
                db.FoodSizes.Remove(foodSize);
                await db.SaveChangesAsync();
            }
        }

        // =====================================
        // VALID NAME (kiểm tra trùng tên FoodName)
        // =====================================
        public async Task<string> ValidName(string foodName)
        {
            var result = await db.FoodSizes.FirstOrDefaultAsync(fs => fs.FoodName == foodName);
            if (result == null) return "";
            return "Tên kích cỡ món ăn bị trùng";
        }
    }
}
