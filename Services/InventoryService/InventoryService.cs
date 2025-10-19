using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Repositories.FoodSizeRepository;
using Ecommerce.Repositories.InventoryRepository;
using Ecommerce.ViewModels;

namespace Ecommerce.Services.InventoryService
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository inventoryRepository;
        private readonly IFoodSizeRepository foodSizeRepository;

        public InventoryService(IInventoryRepository inventoryRepository, IFoodSizeRepository foodSizeRepository)
        {
            this.inventoryRepository = inventoryRepository;
            this.foodSizeRepository = foodSizeRepository;
        }

        public async Task<IEnumerable<InventoryViewModel>> GetAll()
        {
            return await inventoryRepository.GetAll();
        }

        public async Task<InventoryViewModel> GetById(int id)
        {
            return await inventoryRepository.GetById(id);
        }

        public async Task<StatusDTO> Create(Inventory model)
        {
            // Kiểm tra biến thể (FoodSize) có tồn tại không
            var foodSize = await foodSizeRepository.GetById(model.FoodSizeId);
            if (foodSize == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy biến thể món ăn tương ứng" };

            // Kiểm tra số lượng âm
            if (model.Quantity < 0)
                return new StatusDTO { IsSuccess = false, Message = "Số lượng không được âm" };

            await inventoryRepository.Create(model);
            return new StatusDTO { IsSuccess = true, Message = "Thêm tồn kho mới thành công" };
        }

        public async Task<StatusDTO> Update(Inventory model)
        {
            // Kiểm tra tồn tại Inventory
            var existing = await inventoryRepository.GetById(model.InventoryId);
            if (existing == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy tồn kho cần cập nhật" };

            // Kiểm tra biến thể hợp lệ
            var foodSize = await foodSizeRepository.GetById(model.FoodSizeId);
            if (foodSize == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy biến thể món ăn tương ứng" };

            if (model.Quantity < 0)
                return new StatusDTO { IsSuccess = false, Message = "Số lượng không được âm" };

            await inventoryRepository.Update(model);
            return new StatusDTO { IsSuccess = true, Message = "Cập nhật tồn kho thành công" };
        }

        public async Task<StatusDTO> Delete(int id)
        {
            var existing = await inventoryRepository.GetById(id);
            if (existing == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy món ăn cần xóa tồn kho" };

            await inventoryRepository.Delete(id);
            return new StatusDTO { IsSuccess = true, Message = "Xóa tồn kho thành công" };
        }
    }
}
