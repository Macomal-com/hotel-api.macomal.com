using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportModels
{
    public class ReportResponse
    {
        public DataTable ReportData { get; set; } = new DataTable();
        public List<HeaderAndFooter> Columns { get; set; } = new List<HeaderAndFooter>();

        public DataTable TotalTable { get; set; } = new DataTable();

        public DataTable FilteredData { get; set; } = new DataTable();
        public bool HasMore { get; set; }

        public int TotalRows { get; set; }
    }

    public class HeaderAndFooter
    {
        public string AccessorKey { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public string ColumnType { get; set; } = string.Empty;

        public MuiTableHeadCellProps muiTableHeadCellProps { get; set; } = new MuiTableHeadCellProps();
        public MuiTableBodyCellProps muiTableBodyCellProps { get; set; } = new MuiTableBodyCellProps();
        public string Footer { get; set; } = string.Empty;
    }

    public class MuiTableHeadCellProps
    {
        public string Align { get; set; } = string.Empty;
    }

    public class MuiTableBodyCellProps
    {
        public string Align { get; set; } = string.Empty;
    }
}
