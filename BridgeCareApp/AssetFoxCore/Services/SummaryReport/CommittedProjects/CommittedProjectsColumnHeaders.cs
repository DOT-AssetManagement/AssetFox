using System.Collections.Generic;

namespace AssetFoxCore.Services.SummaryReport.CommittedProjects
{
    public class CommittedProjectsColumnHeaders
    {
        public const string Treatment = "TREATMENT";
        public const string Year = "YEAR";
        public const string Budget = "BUDGET";
        public const string Cost = "COST";
        public const string ProjectSource = "PROJECT SOURCE";
        public const string ProjectId = "PROJECT ID";
        public const string Category = "CATEGORY";         

        public static readonly List<string> NoKeyHeaders =
        [            
            Year,
            Treatment,
            Category,
            Budget,
            Cost,
            ProjectSource,
            ProjectId
        ];
    }
}
