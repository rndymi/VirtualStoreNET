using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualStore
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(60, ErrorMessage = "Product name cannot be longer than 60 characters.")]
        [Display(Name = "Product")]
        public string nameProd { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string descripProd { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "0,01", "99999999", ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal price { get; set; }

        [Display(Name = "Stock")]
        [Range(0, 999999, ErrorMessage = "Stock must be 0 or greater.")]
        [ActiveStock(nameof(isActiveProd), ErrorMessage = "If product is active, stock must be greater than 0.")]
        public int stockProd { get; set; }

        [Display(Name = "Enabled")]
        public bool isActiveProd { get; set; }
    }
}