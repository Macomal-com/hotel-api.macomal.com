using FluentValidation;
using Microsoft.AspNetCore.Http;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ReminderHistoryMaster: ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int ReminderId { get; set; }
        public int DaysBefore { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly BillPaidDate { get; set; }
        public string DocumentPath { get; set; } = String.Empty;
        public bool BillPaid { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        [NotMapped]
        public IFormFile? DocumentFile { get; set; }
    }
    public class ReminderHistoryMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public int ReminderId { get; set; }
        public int DaysBefore { get; set; }
        public DateTime DueDate { get; set; }
        public string DocumentPath { get; set; } = String.Empty;
    }
    public class ReminderHistoryValidator : AbstractValidator<ReminderHistoryMaster>
    {
        private readonly DbContextSql _context;
        public ReminderHistoryValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.ReminderId)
                .NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type is required");
            RuleFor(x => x.DueDate)
                .NotNull().WithMessage("Due Date is required")
                .NotEmpty().WithMessage("Due Date is required");
            RuleFor(x => x.DaysBefore)
                .NotNull().WithMessage("Days Before is required")
                .NotEmpty().WithMessage("Days Before is required");

        }
    }
}
