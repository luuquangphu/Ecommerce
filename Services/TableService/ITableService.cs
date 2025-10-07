using Ecommerce.DTO;
using Ecommerce.Models;

namespace Ecommerce.Services.TableService
{
    public interface ITableService
    {
        Task<IEnumerable<Table>> GetAll();
        Task<StatusDTO> Create(Table model);
        Task<StatusDTO> Update(Table model);
        Task<StatusDTO> Delete(int id);
    }
}
