using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualStore.Models.Metadata
{
    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
    }

    public class OrderMetadata
    {
        [Required]
        [Display(Name = "Date")]
        public System.DateTime dateOrder { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Status")]
        public string statusOrder { get; set; }

        [Required]
        [Range(typeof(decimal), "0.00", "999999999", ErrorMessage = "Total amount must be 0 or greater.")]
        [Display(Name = "Total")]
        public decimal totalAmount { get; set; }

        [StringLength(256)]
        [Display(Name = "User")]
        public string userName { get; set; }
    }
}