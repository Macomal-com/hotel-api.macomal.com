﻿using FluentValidation;
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
    public class GroupMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int Other1 { get; set; }
        public int Other2 { get; set; }
        public decimal GST { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
    }

    public class GroupMasterDTO
    {
        [Key]
        public int Id { get; set; }
        public string Code { get; set; } = String.Empty;
        public string GroupName { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public decimal GST { get; set; }
        public decimal IGST { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
    }

    public class GroupValidator : AbstractValidator<GroupMaster>
    {
        private readonly DbContextSql _context;
        public GroupValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x.Code)
                .NotNull().WithMessage("Code is required")
                .NotEmpty().WithMessage("Code is required");
            RuleFor(x => x.GroupName)
                .NotNull().WithMessage("Group Name is required")
                .NotEmpty().WithMessage("Group Name is required");
            RuleFor(x => x)
                .MustAsync(IsUniqueGroupCode)
                .When(x => x.Id == 0)
                .WithMessage("Group Code already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateGroupCode)
                .When(x => x.Id > 0)
                .WithMessage("Group Code already exists");
        }
        private async Task<bool> IsUniqueGroupCode(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            
            return !await _context.GroupMaster.AnyAsync(x => x.Code == groupMaster.Code && x.IsActive == true && x.CompanyId == groupMaster.CompanyId, cancellationToken);
        }


        private async Task<bool> IsUniqueUpdateGroupCode(GroupMaster groupMaster, CancellationToken cancellationToken)
        {
            return !await _context.GroupMaster.AnyAsync(x => x.Code == groupMaster.Code && x.Id != groupMaster.Id && x.IsActive == true && x.CompanyId == groupMaster.CompanyId, cancellationToken);
        }
    }
}
