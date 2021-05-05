using System;
using System.Web.Http;
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
        ///     API endpoint for fetching all networks
        /// </summary>
        /// <returns>IHttpActionResult</returns>
        [HttpGet]
        [Route("api/GetNetworks")]
        [RestrictAccess]
        public IHttpActionResult GetNetworks() {
            return Ok(repo.GetAllNetworks(db));

            // This is an attempt to add Azure AD B2C auth. It will be resumed
            //HasRequiredScopes("read");
            //string name = ClaimsPrincipal.Current.FindFirst("name").Value;
        }

        // Validate to ensure the necessary scopes are present.
        //private void HasRequiredScopes(String permission)
        //{
            //var test = ClaimsPrincipal.Current.FindFirst(scopeElement);
            //if (!ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value.Contains(permission))
            //{
            //    throw new HttpResponseException(new HttpResponseMessage
            //    {
            //        StatusCode = HttpStatusCode.Unauthorized,
            //        ReasonPhrase = $"The Scope claim does not contain the {permission} permission."
            //    });
            //}
        //}
    }
}
