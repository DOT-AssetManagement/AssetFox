using Azure.Identity;
using AssetFoxCore.Security.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AssetFoxCore.Security
{
    public class GraphApiClientService: IGraphApiClientService
    {
        private readonly GraphServiceClient _graphServiceClient;

        public GraphApiClientService(IConfiguration configuration)
        {
            // The Graph credentials live under "AzureAdB2C" (b2c.json) for the B2C path and
            // under "Entra" (entra.json) for the Entra External ID path. Pick the section that
            // matches the active SecurityType so each identity provider has its own settings.
            var sectionName = configuration.GetSection("SecurityType").Value == SecurityConstants.SecurityTypes.Entra
                ? "Entra"
                : "AzureAdB2C";
            var azureAdB2CSection = configuration.GetSection(sectionName);
            if (!azureAdB2CSection.GetChildren().Any())
            {
                return;
            }

            string[] scopes = azureAdB2CSection.GetValue<string>("GraphApi:Scopes")?.Split(' ');
            var tenantId = azureAdB2CSection.GetValue<string>("GraphApi:TenantId");

            // Values from app registration
            var clientId = azureAdB2CSection.GetValue<string>("GraphApi:ClientId");
            var clientSecret = azureAdB2CSection.GetValue<string>("GraphApi:ClientSecret");

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                tenantId, clientId, clientSecret, options);

            _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
        }

        public async Task<List<string>> GetGraphApiUserMemberGroup(string userId)
        {
            var groupNames = new List<string>();
            var objectCollectionResponse = await _graphServiceClient.Users[userId].MemberOf.GetAsync();
            var result = objectCollectionResponse.Value;

            // MemberOf can return directory objects other than groups (e.g. directory roles);
            // OfType filters to groups instead of throwing InvalidCastException on the first non-group.
            foreach (var group in result.OfType<Group>())
            {
                groupNames.Add(group.DisplayName);
            }

            return groupNames;
        }
    }
}
