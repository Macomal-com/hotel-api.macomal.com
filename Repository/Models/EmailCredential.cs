﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class EmailCredential
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Smtp { get; set; } = string.Empty;
        public bool SslTrue { get; set; }
        public string AppPassword { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public int CompanyId { get; set; }
    }

}
