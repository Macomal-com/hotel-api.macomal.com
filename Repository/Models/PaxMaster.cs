﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class PaxMaster
    {
        [Key]
        public int Id { get; set; }
        public int Pax { get; set; }
        public bool IsActive { get; set; }
    }
}
