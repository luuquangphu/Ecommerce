using Ecommerce.Services.VnPayService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        private readonly IVnPayService vnPayService;
    }
}
