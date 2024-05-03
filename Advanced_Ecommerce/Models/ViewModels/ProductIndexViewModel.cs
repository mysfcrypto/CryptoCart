using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Advanced_Ecommerce.Models.ViewModels
{
    public class ProductIndexViewModel
    {
        public int Id { get; set; }
        public string ShortDescrip { get; set; }
        public string LongDescrip { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public int inStock { get; set; }
        public Category Category { get; set; }
        public int Amount { get; set; } = 1;
        public string Total { get=>(Price*Amount).ToString("c", CultureInfo.CreateSpecificCulture("en-US")); }
        public List<Product> RelatedProducts { get; set; }
    }

}
