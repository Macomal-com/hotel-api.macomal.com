using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class PropertyImages
    {
        [Key]
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public string FilePath { get; set; } = String.Empty;
    }
}
