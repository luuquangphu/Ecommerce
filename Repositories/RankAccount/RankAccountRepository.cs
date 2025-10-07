
using Ecommerce.Data;
using Ecommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repositories.RankAccount
{
    public class RankAccountRepository : IRankAccountRepository
    {
        private readonly AppDbContext db;

        public RankAccountRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task CreateAsync(CustomerRank model)
        {
            var customerRank = new CustomerRank
            {
                RankName = model.RankName,
                RankDiscount = model.RankDiscount,
                RankPoint = model.RankPoint,
            };
            db.CustomerRanks.Add(customerRank);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customerRank = await db.CustomerRanks.FindAsync(id);
            db.CustomerRanks.Remove(customerRank);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CustomerRank>> GetAllAsync()
        {
            var lst = await db.CustomerRanks.ToListAsync();
            return lst;
        }

        public async Task<CustomerRank> GetByIdAsync(int id)
        {
            var customerRank = await db.CustomerRanks.FindAsync(id);
            return customerRank;
        }

        public async Task<int> SelectLastestRank()
        {
            var rankId = await db.CustomerRanks.OrderBy(cr => cr.RankPoint).Select(cr => cr.RankId).FirstOrDefaultAsync();
            if(rankId == 0)
            {
                CustomerRank csItem = new CustomerRank
                {
                    RankName = "Chưa có hạng",
                    RankPoint = 0,
                    RankDiscount = 0,
                };
                db.CustomerRanks.Add(csItem);
                await db.SaveChangesAsync();
                rankId = csItem.RankId;
                
            }
            return rankId;
        }

        public async Task UpdateAsync(CustomerRank model)
        {
            var customerRank = await GetByIdAsync(model.RankId);

            customerRank.RankPoint = model.RankPoint;
            customerRank.RankDiscount = model.RankDiscount;
            customerRank.RankName = model.RankName;

            db.CustomerRanks.Update(customerRank);
            await db.SaveChangesAsync();
        }
    }
}
