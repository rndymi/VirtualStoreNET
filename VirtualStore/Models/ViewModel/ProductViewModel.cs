using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VirtualStore.Models.ViewModel
{
    public class ProductViewModel
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "Prodcut Name")]
        public string nameProd { get; set; }

        [Display(Name = "Description")]
        public string descripProd { get; set; }

        [Required]
        [Display(Name = "Price (€)")]
        public decimal price { get; set; }

        [Required]
        [Range(0, 999999)]
        [Display(Name = "Stock quantity")]
        public int stockProd { get; set; }

        [Display(Name = "Active")]
        public bool isActiveProd { get; set; }

        public string imageProd { get; set; }
        public HttpPostedFileBase imageFile { get; set; }

        [Required(ErrorMessage ="Categoty is required.")]
        [Display(Name = "Category")]
        public int? Category_Id { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }


    }
}