namespace Ecommerce.DTO
{
    public class VnPaymentRequestModel
    {
        public int OrderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
