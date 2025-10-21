using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ecommerce.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [DisplayName("Tổng tiền")]
        [Column(TypeName = "decimal(37,1)")]
        public decimal TotalAmount { get; set; }

        [DisplayName("Trạng thái đơn")]
        public string OrderStatus { get; set; }

        [DisplayName("Ngày đặt")]
        public DateTime OrderDate { get; set; }

        [DisplayName("Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        public string? PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
        public virtual ICollection<Revenue_Order> Revenue_Order { get; set; }
    }
}
