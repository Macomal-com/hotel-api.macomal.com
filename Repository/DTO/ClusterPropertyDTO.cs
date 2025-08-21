using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class Response
    {
        public List<ClusterPropertyDTO> Clusters { get; set; } = new List<ClusterPropertyDTO>();

        public List<PropertyDTO> Properties { get; set; } = new List<PropertyDTO>();
    }

    public class ClusterPropertyDTO
    {
        public int ClusterId { get; set; }
        public string ClusterName { get; set; } = string.Empty;
        public bool IsAllProperties { get; set; }
        public List<PropertyDTO> Properties { get; set; } = new List<PropertyDTO>();
    }

    public class PropertyDTO
    {
        public int PropertyId { get; set; }
        public string PropertyName { get; set; } = string.Empty;
    }
}
