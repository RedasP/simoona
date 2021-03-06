﻿using System.ComponentModel.DataAnnotations;
using Shrooms.WebViewModels.ValidationAttributes;

namespace Shrooms.WebViewModels.Models.Book.BookDetails
{
    public class NewBookQuantityViewModel
    {
        [Required]
        [MinValue(1)]
        public int OfficeId { get; set; }

        [Required]
        public int BookQuantity { get; set; }
    }
}
