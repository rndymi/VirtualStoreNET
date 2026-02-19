using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VirtualStore.Models.Metadata
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(40, ErrorMessage = "Category name cannot be longer than 40 characters.")]
        [Display(Name = "Category")]
        public string nameCate { get; set; }
        
        [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        [Display(Name = "Description")]
        public string descripCate { get; set; }

        [Display(Name = "Enabled")]
        public bool isActiveCate { get; set; }
    }
}