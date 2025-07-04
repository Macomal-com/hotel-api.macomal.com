using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class PropertyAuthDto
    {
        public int UserId { get; set; }
        public int PropertyId { get; set; }

        public int ClusterId { get; set; }
        public bool AllProperty { get; set; }
    }
}
