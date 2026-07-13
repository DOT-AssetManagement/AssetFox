using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AssetFoxCore.Services.Branding
{
    public enum ClientLogoKind
    {
        Agency,
        Implementation
    }

    public interface IClientLogoService
    {
        Task<string> SaveAsync(ClientLogoKind logoKind, IFormFile file, CancellationToken cancellationToken = default);

        Task<string> ReadAsDataUrlAsync(ClientLogoKind logoKind, CancellationToken cancellationToken = default);
    }
}
