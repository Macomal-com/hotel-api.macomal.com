using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class AgentCommissionResponse
    {
        public decimal AgentCommissionPercentage { get; set; }
        public decimal AgentCommisionAmount { get; set; }
        public decimal TcsPercentage { get; set; }
        public decimal TdsPercentage { get; set; }
        public decimal TcsAmount { get; set; }
        public decimal TdsAmount { get; set; }
        public string AgentGstType { get; set; } = string.Empty;
    }
}
