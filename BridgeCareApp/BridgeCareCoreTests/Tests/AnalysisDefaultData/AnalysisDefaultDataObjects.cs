using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs.Enums;
using AssetFoxCore.Models.DefaultData;

namespace AssetFoxCoreTests.Tests
{
    public static class AnalysisDefaultDataObjects
    {
        public static AnalysisDefaultData Default =>
            new AnalysisDefaultData
            {
                OptimizationStrategy = OptimizationStrategy.Benefit,
                SpendingStrategy = SpendingStrategy.NoSpending,
            };
    }
}
