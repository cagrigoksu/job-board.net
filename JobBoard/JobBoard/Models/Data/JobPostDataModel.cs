﻿using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models.Data
{
    public class JobPostDataModel
    {
        [Key]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PostDate { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; }
    }
}