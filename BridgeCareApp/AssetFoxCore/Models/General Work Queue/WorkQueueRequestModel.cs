using System;
using AssetFox.Core.DTOs.Enums;

namespace AssetFoxCore.Models.General_Work_Queue
{
    public class WorkQueueRequestModel
    {
        public Guid DomainId { get; set; }
        public WorkType WorkType { get; set; }
    }
}
