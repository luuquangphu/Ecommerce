using Ecommerce.DTO;
using Ecommerce.Services.Customer;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class CustomerApiController : ControllerBase
    {
        private readonly ICustomerService customerService;

        public CustomerApiController(ICustomerService customerService)
        {
            this.customerService = customerService;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllCustomer")]
        public async Task<IActionResult> GetAllCustomer(string? search = null)
        {
            var lstCustomer = await customerService.GetAll(search);
            return Ok(lstCustomer);
        }

        [HttpGet("GetCustomerById")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetCustomerById(string userId)
        {
            var customer = await customerService.GetById(userId);
            if (customer.IsSuccess == false)
            {
                return BadRequest(customer.Message);
            }

            return Ok(customer);
        }

        [HttpPut("UpdateCustomer")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateCustomer(string userId, [FromForm] UpdateCustomerViewModel model, IFormFile? UrlImage)
        {
            var result = await customerService.Update(model, userId, UrlImage);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpDelete("DeleteCustomer")]
        public async Task<IActionResult> DeleteCustomer(string userId)
        {
            var result = await customerService.Delete(userId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }
    }
}
