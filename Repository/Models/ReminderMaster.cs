using FluentValidation;
using Microsoft.EntityFrameworkCore;
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
    }
    public class ReminderValidator : AbstractValidator<ReminderMaster>
    {
        private readonly DbContextSql _context;
        public ReminderValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.ReminderType)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type is required");
            RuleFor(x => x.ReminderMail)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Mail is required")
                .NotEmpty().WithMessage("Mail is required");

        }
    }
    public class ReminderDeleteValidator : AbstractValidator<ReminderMaster>
    {
        private readonly DbContextSql _context;
        public ReminderDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoReminderExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Reminder, History already exists!");
        }
        private async Task<bool> DoReminderExists(ReminderMaster rem, CancellationToken cancellationToken)
        {
            return !await _context.ReminderHistoryMaster
                .Where(x => x.IsActive == true && x.CompanyId == rem.CompanyId && x.ReminderId == rem.Id)
                .AnyAsync();
        }

    }
}
