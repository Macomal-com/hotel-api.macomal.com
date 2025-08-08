using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportModels
{
    public class ReportRequest
    {
        public int CompanyId { get; set; } 
        public int StartPageNumber { get; set; } 
        public int EndPageNumber { get; set; } 
        public Dictionary<string, string> WhereFilters { get; set; } = new Dictionary<string, string>();

        public List<string> OrderByFilter { get; set; } = new List<string>();

        public string OrderBy { get; set; } = string.Empty;

        public bool IsFirstRequest { get; set; } = false;

        public string ReportName { get; set; } = string.Empty;
        public string SpName { get; set; } = string.Empty;

        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }

    public class ColumnsData
    {
        public string ColumnName { get; set; } = string.Empty;
        public string ColumnType { get; set; } = string.Empty;
    }

    public class ReportRequestBody
    {
        public string ReportName { get; set; } = string.Empty;

        
        public int StartPageNumber { get; set; }
        public int EndPageNumber { get; set; }

        

        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;

        public bool IsFirstRequest { get; set; } = false;

        public List<SearchFilters> SearchFilters = new List<SearchFilters>();

        public string IsExcel { get; set; } = "";
        public string ReportHeading { get; set; } = string.Empty;
    }

    public class SearchFilters
    {
        public string Id { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        
         
    }
}
