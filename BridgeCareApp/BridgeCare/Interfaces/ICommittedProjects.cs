using System.Web;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface ICommittedProjects
    {
        void SaveCommittedProjectsFiles(HttpRequest request, BridgeCareContext db, UserInformationModel userInformation);

        byte[] ExportCommittedProjects(int simulationId, int networkId, BridgeCareContext db, UserInformationModel userInformation);
    }
}
