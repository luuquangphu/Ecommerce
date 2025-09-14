using Ecommerce.Services.Account;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly IAccountService accountService;

        public AccountApiController(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model, IFormFile? image)
        {
            var result = await accountService.RegisterAsync(model, image);
            return result.isSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }
    }
}
