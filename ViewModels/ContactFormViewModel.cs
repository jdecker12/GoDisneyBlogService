using System.ComponentModel.DataAnnotations;

namespace GoDisneyBlog.ViewModels
{
    public class ContactFormViewModel
    {
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(30, ErrorMessage = "{0} length is between {2} and {1}", MinimumLength = 2)]
        public string? name { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(30, ErrorMessage = "{0} length is between {2} and {1}", MinimumLength = 2)]
        public string? email { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(30, ErrorMessage = "{0} length is between {2} and {1}", MinimumLength = 2)]
        public string? message { get; set; }
    }
}
