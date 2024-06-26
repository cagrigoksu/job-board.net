﻿using System.ComponentModel.DataAnnotations;

namespace JobBoard.Models.Data
{
    public class UserDataModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; } = null!;
 
        [Required]
        public string PasswordSalt { get; set; } = null!;
        [Required] public string PasswordHash { get; set; } = null!;
        public bool CompanyUser { get; set; }
        public DateTime LogOnDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; }
        public int DeleteUser { get; set; }
    }
}
