using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface ISectionLocator
    {
        SectionLocationModel Locate(SectionModel section, BridgeCareContext db);
    }
}
