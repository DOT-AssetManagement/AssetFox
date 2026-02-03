using System;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.DTOs
{
    public class SimulationOutputJsonDTO: BaseDTO
    {
        public string Output { get; set; }

        public SimulationOutputEnum OutputType { get; set; }
    }    
}
