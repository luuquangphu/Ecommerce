namespace Ecommerce.Repositories.RankAccount
{
    public interface IRankAccountRepository
    {
        Task<int> SelectLastestRank();
    }
}
