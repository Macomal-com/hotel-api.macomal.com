﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repository.Models
{
    public class OwnerMaster : ICommonProperties
    {
        [Key]
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        [NotMapped]
        public IFormFile? AgreementDocuments { get; set; }
        public string AgreementDocumentsPath { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string CreatedBy { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class OwnerMasterDTO
    {
        [Key]
        public string OwnerName { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        [NotMapped]
        public IFormFile? AgreementDocuments { get; set; }
        public string AgreementDocumentsPath { get; set; } = String.Empty;
        public double CommissionPercentage { get; set; }
    }
}
