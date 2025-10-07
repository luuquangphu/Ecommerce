using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Repositories.TableRepository;

namespace Ecommerce.Services.TableService
{
    public class TableService : ITableService
    {
        private readonly ITableRepository tableRepository;

        public TableService(ITableRepository tableRepository)
        {
            this.tableRepository = tableRepository;
        }

        public async Task<StatusDTO> Create(Table model)
        {
            if(model == null) 
                return new StatusDTO { IsSuccess = false, Message = "Model rỗng"};
            if(model.NumberOfSeats <= 0)
                return new StatusDTO { IsSuccess = false, Message = "Chỗ ngồi không thể nhỏ hơn hoặc bằng 0" };
            await tableRepository.Create(model);
            return new StatusDTO { IsSuccess = true, Message = $"Tạo bàn: {model.TableName} thành công" };
        }

        public async Task<StatusDTO> Delete(int id)
        {
            if(id <= 0 )
                return new StatusDTO { IsSuccess = false, Message = "Mã bàn không hợp lệ" };
            var table = await tableRepository.GetById(id);
            if(table == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy bàn cần xóa" };
            await tableRepository.Delete(id);
            return new StatusDTO { IsSuccess = true, Message = "Xóa bản thành công" };
        }

        public async Task<IEnumerable<Table>> GetAll() => await tableRepository.GetAll();

        public async Task<StatusDTO> Update(Table model)
        {
            if (model.TableId <= 0)
                return new StatusDTO { IsSuccess = false, Message = "Mã bàn không hợp lệ" };
            var table = await tableRepository.GetById(model.TableId);
            if (table == null)
                return new StatusDTO { IsSuccess = false, Message = "Không tìm thấy bàn cần cập nhật" };
            await tableRepository.Update(model);
            return new StatusDTO { IsSuccess = true, Message = $"Cập nhật bàn: {model.TableName} thành công" };
        }
    }
}
