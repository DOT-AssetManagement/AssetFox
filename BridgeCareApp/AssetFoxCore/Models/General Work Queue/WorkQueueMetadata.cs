using System;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCore.Models
{
    public class WorkQueueMetadata
    {
        public WorkType WorkType { get; set; }
        public DomainType DomainType { get; set; }
        public string PreviousRunTime { get; set; }
        public Guid DomainId { get; set; }
    }
}
