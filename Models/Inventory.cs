using System.ComponentModel.DataAnnotations;
using System;

namespace Ecommerce.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }

        public int FoodSizeId { get; set; }
        public FoodSize FoodSize { get; set; }

        public string Unit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng không được là 1 số âm và lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
