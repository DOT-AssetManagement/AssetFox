using System.Collections.Generic;
using Microsoft.Graph.Models;

namespace AssetFoxCore.Services.SummaryReport.CommittedProjects
{
    public class CommittedProjectsColumnHeaders
    {
        public const string BRKey = "BRKEY_"; //TODO import do not refresh after done
        public const string BMSID = "BMSID"; //TODO import do not refresh after done
        public const string CRS = "PAVESEG"; //"CRS";  //TODO use key settings where it is been used! Falis for Import //TODO import do not refresh after done
        public const string Treatment = "TREATMENT";
        public const string Year = "YEAR";
        public const string Budget = "BUDGET";
        public const string Cost = "COST";
        public const string ProjectSource = "PROJECTSOURCE";
        public const string ProjectSourceId = "PROJECTSOURCEID";
        public const string Category = "CATEGORY"; 

        public static readonly List<string> AllHeaders = new()
        {
            BRKey,
            BMSID,
            CRS,
            Treatment,
            Year,
            Budget,
            Cost,
            ProjectSource,
            ProjectSourceId,
            Category
        };

        public static readonly List<string> NoKeyHeaders = new()
        {
            Treatment,
            Year,
            Budget,
            Cost,
            ProjectSource,
            ProjectSourceId,
            Category
        };

        public static readonly List<string> BridgeHeaders = new()
        {
            BRKey,
            BMSID,
            Treatment,
            Year,
            Budget,
            Cost,
            ProjectSource,
            ProjectSourceId,
            Category
        };

        public static readonly List<string> RoadHeaders = new()
        {
            CRS,
            Treatment,
            Year,
            Budget,
            Cost,
            ProjectSource,
            ProjectSourceId,
            Category
        };

        public static readonly List<string> BridgeKeyHeaders = new()
        {
            BRKey,
            BMSID,
        };

        public static readonly List<string> RoadKeyHeaders = new()
        {
            CRS
        };
    }
}
