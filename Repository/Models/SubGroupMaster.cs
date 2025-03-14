﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class SubGroupMaster : ICommonParams
    {
        [Key]
        public int SubGroupId { get; set; }
        public string SubGroupName { get; set; } = String.Empty;
        public string Descriptoion { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class SubGroupMasterDTO
    {
        [Key]
        public string SubGroupName { get; set; } = String.Empty;
        public int GroupId { get; set; }
        public string Descriptoion { get; set; } = String.Empty;

    }
}
