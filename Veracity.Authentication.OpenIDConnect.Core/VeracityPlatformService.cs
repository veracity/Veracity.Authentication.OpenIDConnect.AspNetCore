namespace Veracity.Authentication.OpenIDConnect.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Authentication;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.Identity.Client;

    public class VeracityPlatformService
    {
        private readonly VeracityIntegrationOptions _veracityIntegrationOptions;
        private readonly HttpContext _httpContext;
        public VeracityPlatformService(HttpClient client, IVeracityIntegrationConfigService configurationServices, IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext.HttpContext;
            _veracityIntegrationOptions = configurationServices.GetVeracityIntegrationConfig();
            client.BaseAddress = new Uri(_veracityIntegrationOptions.VeracityPlatformServiceUrl);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _veracityIntegrationOptions.VeracityPlatformServiceKey);
            Client = client;
        }

        public HttpClient Client { get; }

        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync()
        {
            var accessToken = await GetAccessTokenAsync();
            return new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var scope = _veracityIntegrationOptions.VeracityPlatformServiceScopes.Split(' ');
            TokenCache userTokenCache = new MSALSessionCache(_httpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(_veracityIntegrationOptions.ClientId,
                _veracityIntegrationOptions.Authority,
                _veracityIntegrationOptions.RedirectUri,
                new ClientCredential(_veracityIntegrationOptions.ClientSecret),
                userTokenCache,
                null);
            try
            {
                IEnumerable<IAccount> accounts = await cca.GetAccountsAsync();
                IAccount firstAccount = accounts.FirstOrDefault();
                var result = await cca.AcquireTokenSilentAsync(
                                 scope,
                                 firstAccount,
                                 _veracityIntegrationOptions.Authority,
                                 false);
                return result.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                // Cannot find any cache user in memory, you should sign out and login again.
                throw new AuthenticationException("Cannot find login user credential, please sign out and login again");
            }
        }
    }
}
