namespace Ecommerce.DTO
{
    public class PaymentConfirmDTO
    {
        public int OrderId { get; set; }
        public string TableName { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public IEnumerable<OrderDetailDTO> OrderDetail { get; set; }
    }
}
