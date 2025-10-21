using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace Ecommerce.Models
{
    public class OrderDetails
    {
        public int FoodSizeId { get; set; }
        public FoodSize FoodSize { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        [DisplayName("Số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng không được là 1 số âm và lớn hơn 0.")]
        public int Count { get; set; }
    }
}
