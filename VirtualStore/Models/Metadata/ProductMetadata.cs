using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualStore.Models.Metadata
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(60, ErrorMessage = "Product name cannot be longer than 60 characters.")]
        [Display(Name = "ProducT")]
        public string nameProd { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        [Display(Name = "Description")]
        public string descripProd { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal price { get; set; }

        [Required(ErrorMessage = "Stock is required.")]
        [Range(0, 999999, ErrorMessage = "Stock must be 0 or greater.")]
        [Display(Name = "Stock")]
        public int stockProd { get; set; }

        [Display(Name = "Enabled")]
        public bool isActiveProd { get; set; }

    }
}