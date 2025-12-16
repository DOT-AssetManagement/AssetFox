using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IAnnouncementRepository
    {
        List<AnnouncementDTO> Announcements();

        void UpsertAnnouncement(AnnouncementDTO dto);

        void DeleteAnnouncement(Guid announcementId);
    }
}
