using Azure;
using Ecommerce.DTO;
using Ecommerce.Repositories.OrderRepository;
using Ecommerce.Repositories.RevenueRepository;
using Ecommerce.Services.OrderService;
using Ecommerce.Services.VnPayService;

namespace Ecommerce.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentService paymentService;
        private readonly IVnPayService vnPayService;
        private readonly IOrderRepository orderRepository;
        private readonly IRevenueRepository revenueRepository;

        public PaymentService(IPaymentService paymentService, IVnPayService vnPayService, IOrderRepository orderRepository, IRevenueRepository revenueRepository)
        {
            this.paymentService = paymentService;
            this.vnPayService = vnPayService;
            this.orderRepository = orderRepository;
            this.revenueRepository = revenueRepository;
        }

        public async Task<string> CreatePaymentUrlVnPay(HttpContext httpContext ,VnPaymentRequestModel model)
        {
            return vnPayService.CreatePaymentUrl(httpContext, model);
        }

        public async Task<object> PaymentCallbackAsync(IQueryCollection query)
        {
            var response = vnPayService.PaymentExecute(query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                return new
                {
                    success = false,
                    message = "Lỗi thanh toán VNPay!",
                    response = new { response?.VnPayResponseCode }
                };
            }

            var order = await orderRepository.GetById(response.OrderId);
            if (order == null)
            {
                return new { success = false, message = "Đơn hàng không tồn tại." };
            }

            // Cập nhật trạng thái đơn hàng
            order.PaymentStatus = "Đã thanh toán";

            // Lấy và cập nhật doanh thu
            int revenueId = await revenueRepository.CheckRevenue();
            var revenue = await revenueRepository.GetRevenueByIdAsync(revenueId);
            if (revenue != null)
            {
                revenue.TotalAmount += order.TotalAmount;
                await revenueRepository.addRevenue_Orders(order.OrderId, revenueId);
                await revenueRepository.UpdateRevenueAsync(revenue);
            }

            await orderRepository.UpdatePaymentStatusOrderAsync(order);

            var orderDetail = await orderRepository.GetOrderDetailsByIdAsync(order.OrderId);

            return new
            {
                success = true,
                message = "Thanh toán thành công!",
                orderDetail
            };
        }

        public async Task<object> ConfirmOrderPayByCash(int orderId)
        {
            var order = await orderRepository.GetById(orderId);
            if (order == null)
            {
                return new { success = false, message = "Đơn hàng không tồn tại." };
            }

            // Cập nhật trạng thái đơn hàng
            order.PaymentStatus = "Đã thanh toán";

            // Lấy và cập nhật doanh thu
            int revenueId = await revenueRepository.CheckRevenue();
            var revenue = await revenueRepository.GetRevenueByIdAsync(revenueId);
            if (revenue != null)
            {
                revenue.TotalAmount += order.TotalAmount;
                await revenueRepository.addRevenue_Orders(order.OrderId, revenueId);
                await revenueRepository.UpdateRevenueAsync(revenue);
            }

            await orderRepository.UpdatePaymentStatusOrderAsync(order);

            var orderDetail = await orderRepository.GetOrderDetailsByIdAsync(order.OrderId);

            return new
            {
                success = true,
                message = "Thanh toán thành công!",
                orderDetail
            };
        }
    }
}
