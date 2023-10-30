using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.ViewModels
{
    public class CardContentsViewModel
    {
        public string? paraOne { get; set; }
        public string? paraTwo { get; set; }
        public string? paraThree { get; set; }
        public string? paraFour { get; set; }
        [StringLength(150, ErrorMessage = "{0} length is between {2} and {1}", MinimumLength = 25)]

        public CardContentsViewModel? GetCardContentsViewModel { get; set; }
    }
}
