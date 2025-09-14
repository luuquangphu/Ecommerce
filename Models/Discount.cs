using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models
{
    public class Discount
    {
        [Key]
        public string DiscountId { get; set; }

        public string DiscountName { get; set; }
        
        public required string DiscountCategory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public required decimal DiscountPrice { get; set; }

        public DateTime DateEnd { get; set; }

        public DateTime DateStart { get; set; }

        //False: ngừng áp dụng, true: áp dụng
        public bool DiscountStatus { get; set; }
        
        public virtual ICollection<Discount_Customer> Discount_Customer { get; set; }
    }
}
