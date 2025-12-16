using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetFoxCore.Security.Interfaces
{
    public interface IGraphApiClientService
    {
        Task<List<string>> GetGraphApiUserMemberGroup(string userId);
    }
}
