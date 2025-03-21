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
    public class RoomCategoryMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int? BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int DefaultPax { get; set; }
        public int ExtraBed { get; set; }

    }
    public class RoomCategoryMasterDTO
    {
        [Key]
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;
        public int DefaultPax { get; set; }
        public int ExtraBed { get; set; }

    }

    public class RoomCategoryValidator : AbstractValidator<RoomCategoryMaster>
    {
        private readonly DbContextSql _context;
        public RoomCategoryValidator(DbContextSql context)
        {
            _context = context;


            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Room Type is required")
                .NotNull().WithMessage("Room Type is required");

            RuleFor(x => x.MinPax)
               .NotEmpty().WithMessage("Min Pax is required")
               .NotNull().WithMessage("Min Pax is required")
               .GreaterThan(0).WithMessage("Min Pax should be greater than 0")
               .LessThan(x=>x.MaxPax).WithMessage("Min Pax should be less than max pax");

            RuleFor(x => x.MaxPax)
               .NotEmpty().WithMessage("Max Pax is required")
               .NotNull().WithMessage("Max Pax is required")
               .GreaterThan(0).WithMessage("Max Pax should be greater than 0")
               ;

            RuleFor(x => x.NoOfRooms)
               .NotEmpty().WithMessage("No of Rooms is required")
               .NotNull().WithMessage("No of Rooms is required")
               .GreaterThan(0).WithMessage("No of Rooms should be greater than 0")
               ;

            RuleFor(x => x.ExtraBed)
               .NotEmpty().WithMessage("Extra Bed is required")
               .NotNull().WithMessage("Extra Bed is required")
               .GreaterThan(0).WithMessage("Extra Bed should be greater than 0")
               ;

            RuleFor(x => x)
                .MustAsync(isUniqueCategory)
                .When(x => x.Id == 0)
                .WithMessage("Category already exists with same name");
            RuleFor(x => x)
               .MustAsync(isUniqueUpdateCategory)
               .When(x => x.Id > 0)
               .WithMessage("Category already exists with same name");

        }

        private async Task<bool> isUniqueCategory(RoomCategoryMaster master, CancellationToken cancellationToken)
        {
           
                return !await _context.RoomCategoryMaster.AnyAsync(x => x.Type == master.Type && x.CompanyId == master.CompanyId  &&  x.IsActive == true, cancellationToken);
           

        }

        private async Task<bool> isUniqueUpdateCategory(RoomCategoryMaster master, CancellationToken cancellationToken)
        {

            return !await _context.RoomCategoryMaster.AnyAsync(x => x.Type == master.Type && x.CompanyId == master.CompanyId && x.IsActive == true && x.Id != master.Id, cancellationToken);


        }
    }
}
