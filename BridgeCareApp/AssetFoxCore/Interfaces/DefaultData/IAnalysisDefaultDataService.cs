using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetFoxCore.Models.DefaultData;

namespace AssetFoxCore.Interfaces.DefaultData
{
    public interface IAnalysisDefaultDataService
    {
        Task<AnalysisDefaultData> GetAnalysisDefaultData();
    }
}
