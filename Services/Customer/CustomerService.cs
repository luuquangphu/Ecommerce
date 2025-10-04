using Ecommerce.Data;
using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Repositories.CustomerRepository;
using Ecommerce.Services.Vaild;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Services.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository customerRepository;
        private readonly UserManager<Users> userManager;

        public CustomerService(ICustomerRepository customerRepository, UserManager<Users> userManager)
        {
            this.customerRepository = customerRepository;
            this.userManager = userManager;
        }

        public async Task<List<CustomerDTO>> GetAll(string? search = null)
        {
            var lstCustomer = await customerRepository.GetAll(search);
            return lstCustomer;
        }

        public async Task<CustomerResultDTO> GetById(string userId)
        {
            var customerItem = await customerRepository.GetById(userId);
            if (customerItem == null)
            {
                return new CustomerResultDTO
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy khách hàng."
                };
            }

            return new CustomerResultDTO
            {
                IsSuccess = true,
                Message = "Lấy thông tin thành công.",
                Customer = new CustomerDTO
                {
                    Name = customerItem.Name,
                    Email = customerItem.Email,
                    PhoneNumber = customerItem.PhoneNumber,
                    Gender = customerItem.Gender,
                    DateOfBirth = customerItem.DateOfBirth,
                    UrlImage = customerItem.UrlImage,
                    RankName = customerItem.CustomerRank.RankName,
                }
            };
        }

        public async Task<StatusDTO> Update(UpdateCustomerViewModel model, string userId, IFormFile? UrlImage)
        {
            var customer = await customerRepository.GetById(userId);
            if (customer == null)
                return new StatusDTO { IsSuccess = false, Message = $"Không tìm thấy khách hàng với ID: {userId}"};

            // Kiểm tra email
            var existingUserByEmail = await userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null && model.Email != customer.Email)
                return new StatusDTO {IsSuccess = false, Message = "Email đã tồn tại" };

            // Kiểm tra số điện thoại
            var existingPhoneCustomer = await customerRepository.GetCustomerByPhoneAsync(model.PhoneNumber, customer.Id);
            if (existingPhoneCustomer != null)
                return new StatusDTO { IsSuccess = false, Message = "Số điện thoại đã tồn tại"};

            var customerItem = await customerRepository.Update(model, userId, UrlImage);

            if (customerItem == null) return new StatusDTO { IsSuccess = false, Message = "Không cập nhật được thông tin Customer" };

            return new StatusDTO { IsSuccess = true, Message = "Cập nhật thành công" };
        }

        public async Task<StatusDTO> Delete(string userId)
        {
            var customer = await customerRepository.GetById(userId);
            if (customer == null)
                return new StatusDTO { IsSuccess = false, Message = $"Không tìm thấy khách hàng với ID: {userId}" };

            var hasOrders = await customerRepository.IsCustomerCart(userId);
            if (hasOrders)
                return new StatusDTO { IsSuccess = false, Message = "Không thể xoá khách hàng đã có đơn hàng!" };

            var  result = await customerRepository.Delete(customer);
            if (result.IsSuccess == false)
            {
                return new StatusDTO { IsSuccess = false, Message = result.Message };
            }
            return new StatusDTO {IsSuccess = true , Message = "Xoá khách hàng thành công" };
        }
    }
}
