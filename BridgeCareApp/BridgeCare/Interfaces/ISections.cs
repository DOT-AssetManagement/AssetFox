using System.Linq;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface ISections
    {
        IQueryable<SectionModel> GetSections(int networkId, BridgeCareContext db);

        int GetBrKey(int networkID, int sectionID, BridgeCareContext db);

        int GetSectionId(int networkID, int brKey, BridgeCareContext db);
    }
}
