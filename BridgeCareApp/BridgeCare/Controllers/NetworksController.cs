using BridgeCare.Interfaces;
using BridgeCare.Security;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;

namespace BridgeCare.Controllers
{
    public class NetworksController : ApiController
    {
        private readonly INetwork repo;
        private readonly BridgeCareContext db;

        // OWIN auth middleware constants
        public const string scopeElement = "http://schemas.microsoft.com/identity/claims/scope";
        public const string objectIdElement = "http://schemas.microsoft.com/identity/claims/objectidentifier";


        public NetworksController(INetwork repo, BridgeCareContext db)
        {
            this.repo = repo ?? throw new ArgumentNullException(nameof(repo));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// API endpoint for fetching all networks
        /// </summary>
        /// <returns>IHttpActionResult</returns>
        [HttpGet]
        [Route("api/GetNetworks")]
        [RestrictAccess]
        [Authorize]
        public IHttpActionResult GetNetworks() {
            HasRequiredScopes("read");
            string name = ClaimsPrincipal.Current.FindFirst("name").Value;
            return Ok(repo.GetAllNetworks(db));
        }

        // Validate to ensure the necessary scopes are present.
        private void HasRequiredScopes(String permission)
        {
            var userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            //if (!ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value.Contains(permission))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage
            //    {
            //        StatusCode = HttpStatusCode.Unauthorized,
            //        ReasonPhrase = $"The Scope claim does not contain the {permission} permission."
            //    });
            //}
        }
    }
}
