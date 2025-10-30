using Ecommerce.Data;
using Ecommerce.DTO;
using Ecommerce.Services.PaymentService;
using Ecommerce.Services.VnPayService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        private readonly IVnPayService vnPayService;
        private readonly IPaymentService paymentService;

        public PaymentApiController(IVnPayService vnPayService, IPaymentService paymentService)
        {
            this.vnPayService = vnPayService;
            this.paymentService = paymentService;
        }

        [HttpPost("CreatePaymentVnPay")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreatePaymentVnPay([FromBody] VnPaymentRequestModel model)
        {
            var result = await paymentService.CreatePaymentUrlVnPay(HttpContext, model);
            return Ok(result);
        }

        [HttpGet("PaymentCallback")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PaymentCallback()
        {
            try
            {
                var result = await paymentService.PaymentCallbackAsync(Request.Query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xử lý callback thanh toán.",
                    error = ex.Message
                });
            }
        }

        public async Task<IActionResult> ConfirmOrderPayByCash(int orderId)
        {
            var result = await paymentService.ConfirmOrderPayByCash(orderId);
            return Ok(result);
        }
    }
}
