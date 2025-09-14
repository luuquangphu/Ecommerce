using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ecommerce.Models
{
    public class Cart
    {
        [Key]
        public string CartId { get; set; }

        [DisplayName("Trạng thái")]
        public string CartStatus { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        public int TableId { get; set; }
        public Table Table { get; set; }

        public virtual ICollection<CartDetails> CartDetails { get; set; }
    }
}
