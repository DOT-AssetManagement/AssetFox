using System;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class AnnouncementDTO : BaseDTO
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
