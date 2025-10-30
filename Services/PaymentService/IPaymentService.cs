using Ecommerce.DTO;

namespace Ecommerce.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentUrlVnPay(HttpContext httpContext, VnPaymentRequestModel model);
        Task<object> PaymentCallbackAsync(IQueryCollection query);
        Task<object> ConfirmOrderPayByCash(int orderId);
    }
}
