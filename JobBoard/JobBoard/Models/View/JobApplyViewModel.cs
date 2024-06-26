﻿using JobBoard.Enums;

namespace JobBoard.Models.View
{
    public class JobApplyViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int CompanyId { get; set; }
        public int SectorId { get; set; }
        public int LevelId { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime PostDate { get; set; }
        public bool isApplied { get; set; }

        public ApplicationStatusEnum? Status { get; set; }
        public IFormFile CV { get; set; }
        public IFormFile MotivationLetter { get; set; }
        public bool CompanyUser { get; set; }
    }
}
