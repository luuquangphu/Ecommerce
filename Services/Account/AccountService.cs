using Ecommerce.Repositories.Account;
using Ecommerce.Repositories.RankAccount;
using Ecommerce.Services.Vaild;
using Ecommerce.ViewModels;

namespace Ecommerce.Services.Account
{
    public class AccountService : IAccountService
    {
        private readonly IVaildService vaildService;
        private readonly IRankAccountRepository rankAccountRepository;
        private readonly IAccountRepository accountRepository;

        public AccountService(IVaildService vaildService, IRankAccountRepository rankAccountRepository, IAccountRepository accountRepository)
        {
            this.vaildService = vaildService;
            this.rankAccountRepository = rankAccountRepository;
            this.accountRepository = accountRepository;
        } 

        public async Task<(bool isSuccess, string Message)> RegisterAsync(RegisterViewModel model, IFormFile imageFile)
        {
            if (!vaildService.KiemTraNgaySinh(model.DateOfBirth, out string errorMessage))
                return (false, errorMessage);

            if (!vaildService.checkEmail(model.Email))
                return (false, "Email đã tồn tại.");

            //Nếu không gán ảnh thì chọn ảnh mặc định lưu trong hệ thống
            var imagePath = string.IsNullOrEmpty(model.UrlImage)
            ? "/Image/GuestUser/GuestUser.jpg"
            : model.UrlImage;

            //Lưu ảnh người dùng vào trong project
            if (imageFile != null)
                imagePath = await vaildService.SaveImage(imageFile, model.Name, model.PhoneNumber, "");

            //Kiểm tra rank và lấy rankId
            int rankId = await rankAccountRepository.SelectLastestRank();

            //Tạo tài khoản
            var (isSuccess, Message) = await accountRepository.CreateAsync(model, rankId, imagePath);
            if (isSuccess == false)
            {
                return (false, "Đăng ký thất bại");
            }
            return (true, "Đăng ký thành công");
        }
    }
}
