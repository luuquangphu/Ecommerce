namespace Ecommerce.Models
{
    public class Revenue_Order
    {
        public string RevenueId { get; set; }
        public Revenue Revenue { get; set; }

        public string OrderId { get; set; }
        public Order Order { get; set; }
    }
}