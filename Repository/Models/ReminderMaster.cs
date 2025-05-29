using FluentValidation;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ReminderMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string ReminderType { get; set; } = String.Empty;
        public string HolderName { get; set; } = String.Empty;
        public string ContactNo { get; set; } = String.Empty;
        public string WhatsappContactNo { get; set; } = String.Empty;
        public string UniqueCaNo { get; set; } = String.Empty;
        public string ReminderMail { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateOnly ReminderDate { get; set; } = DateOnly.MinValue;
        public string ReminderTime { get; set; } = String.Empty;
        public DateTime ReminderDatetime { get; set; } = new DateTime(1900, 01, 01);
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class ReminderMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public string ReminderType { get; set; } = String.Empty;
        public string HolderName { get; set; } = String.Empty;
        public string ContactNo { get; set; } = String.Empty;
        public string WhatsappContactNo { get; set; } = String.Empty;
        public string UniqueCaNo { get; set; } = String.Empty;
        public string ReminderMail { get; set; } = String.Empty;
        public DateOnly ReminderDate { get; set; } = DateOnly.MinValue;
        public string ReminderTime { get; set; } = String.Empty;
        public DateTime ReminderDatetime { get; set; } = new DateTime(1900, 01, 01);
    }
    public class ReminderValidator : AbstractValidator<ReminderMaster>
    {
        private readonly DbContextSql _context;
        public ReminderValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.ReminderType)
                .NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type is required");
            RuleFor(x => x.ReminderMail)
                .NotNull().WithMessage("Mail is required")
                .NotEmpty().WithMessage("Mail is required");

        }
    }
}
