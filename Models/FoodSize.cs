using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Ecommerce.Models
{
    public class FoodSize
    {
        public int FoodSizeId { get; set; }

        public string MenuId { get; set; }
        public Menu Menu { get; set; }

        public string FoodName { get; set; }// S,M,L

        [Column(TypeName = "decimal(18,2)")]
        [DisplayName("Giá bán")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá bán của 1 món ăn không được là 1 số âm!")]
        [Required(ErrorMessage = "Không được để trống giá bán!")]
        public decimal Price { get; set; } 

        public int SortOrder { get; set; }// Thứ tự
        public virtual ICollection<CartDetails> CartDetails { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}
