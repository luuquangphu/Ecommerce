
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
    }
}
