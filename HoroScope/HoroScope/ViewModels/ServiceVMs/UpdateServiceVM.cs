﻿using HoroScope.Models;
using System.ComponentModel.DataAnnotations;

namespace HoroScope.ViewModels
{
    public class UpdateServiceVM
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public List<ServiceCategory>? ServiceCategories { get; set; }
        public decimal? Price { get; set; }
        public bool IsFree { get; set; }

    }
}
