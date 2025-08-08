namespace hotel_api.Constants
{
    public class ReportConstants
    {
        public static readonly Dictionary<string, string> Reports = new()
        {
            //Level 1 reports
            { "PRODUCTIONIN", "Stock_Reports_L1" },
            { "PRODUCTIONOUT", "Stock_Reports_L1" },

            //Level 2 reports
            { "PRODUCTIONIN_VIEW", "Stock_Reports_L2" },
            { "PRODUCTIONOUT_VIEW", "Stock_Reports_L2" },

            //Level 3 reports
             { "PRODUCTIONIN_VIEW_VIEW", "Stock_Reports_L3" },
             { "PRODUCTIONOUT_VIEW_VIEW", "Stock_Reports_L3" },

            //Excel
            { "PRODUCTIONIN_EXCEL", "ProductionIn_Excel_Date" },
        };
    }
}
