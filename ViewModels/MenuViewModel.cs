using Ecommerce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Ecommerce.ViewModels
{
    public class MenuViewModel
    {
        public int MenuId { get; set; }

        public string MenuName { get; set; }

        public string? Detail { get; set; }

        public string MenuCategoryName { get; set; }
    }
}
