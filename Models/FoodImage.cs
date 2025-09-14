namespace Ecommerce.Models
{
    public class FoodImage
    {
        public int FoodImageId { get; set; }

        public string? UrlImage { get; set; }

        public string MenuId { get; set; }
        public Menu Menu { get; set; }

        public int SortOrder { get; set; }// Thứ tự ảnh

        public bool MainImage { get; set; }
    }
}
