using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualStore.Models.Metadata
{
    [MetadataType(typeof(OrderDetailMetadata))]
    public partial class OrderDetail
    {
    }

    public class OrderDetailMetadata
    {
        [Required]
        [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
        [Display(Name = "Quantity")]
        public int quantity { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Unit price must be greater than 0.")]
        [Display(Name = "Unit Price")]
        public decimal unitPrice { get; set; }

        [Required]
        [Range(typeof(decimal), "0.00", "999999999", ErrorMessage = "Total must be 0 or greater.")]
        [Display(Name = "Total")]
        public decimal orderTotal { get; set; }
    }
}