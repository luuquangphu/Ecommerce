using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.TableRepository
{
    public class TableRepository : ITableRepository
    {
        private readonly AppDbContext db;

        public TableRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task Create(Table model)
        {
            var table = new Table
            {
                TableName = model.TableName,
                TableStatus = "Trống",
                NumberOfSeats = model.NumberOfSeats,
                QRCodePath = "",
            };
            db.Tables.Add(table);
            await db.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var table = await GetById(id);
            db.Tables.Remove(table);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Table>> GetAll()
        {
            var lst = await db.Tables.ToListAsync();
            return lst;
        }

        public async Task<Table> GetById(int id)
        {
            var table = await db.Tables.FindAsync(id);
            return table;
        }

        public async Task Update(Table model)
        {
            var table = await GetById(model.TableId);

            table.TableName = model.TableName;
            table.TableStatus = model.TableStatus;
            table.NumberOfSeats = model.NumberOfSeats;

            await db.SaveChangesAsync();
        }
    }
}
