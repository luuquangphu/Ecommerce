namespace Ecommerce.DTO
{
    public class OrderUserDTO
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public string PaymentStatus { get; set; }
        public IEnumerable<OrderDetailDTO> OrderDetail { get; set; }
    }
}
