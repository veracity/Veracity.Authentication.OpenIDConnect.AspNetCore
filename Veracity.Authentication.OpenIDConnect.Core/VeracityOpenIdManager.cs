using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Veracity.Authentication.OpenIDConnect.Core
{
    public interface IVeracityOpenIdManager
    {
        Action<OpenIdConnectOptions> GetOpenIdOptions();
    }

    public class VeracityOpenIdManager : IVeracityOpenIdManager
    {
        private readonly Action<OpenIdConnectOptions> _veracityOpenIdOption;
        private readonly VeracityIntegrationOptions _azureAdB2COptions;

        public VeracityOpenIdManager(IVeracityIntegrationConfigService configurationServices)
        {
            _azureAdB2COptions = configurationServices.GetVeracityIntegrationConfig();
            _veracityOpenIdOption = o =>
                {
                    o.Authority = _azureAdB2COptions.Authority;
                    o.ClientId = _azureAdB2COptions.ClientId;
                    o.UseTokenLifetime = true;
                    o.ClientSecret = _azureAdB2COptions.ClientSecret;
                    o.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = OnRedirectToIdentityProviderAsync,
                        OnAuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync,
                    };
                };
        }

        public Action<OpenIdConnectOptions> GetOpenIdOptions()
        {
            return _veracityOpenIdOption;
        }

        private async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedContext context)
        {
            // Use MSAL to swap the code for an access token, extract the code from the response notification
            var code = context.ProtocolMessage.Code;
            TokenCache userTokenCache = new MSALSessionCache(context.HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cca = new ConfidentialClientApplication(
                _azureAdB2COptions.ClientId,
                _azureAdB2COptions.Authority,
                _azureAdB2COptions.RedirectUri,
                new ClientCredential(_azureAdB2COptions.ClientSecret),
                userTokenCache,
                null);
            await cca.AcquireTokenByAuthorizationCodeAsync(code, _azureAdB2COptions.VeracityPlatformServiceScopes.Split(' '));
        }

        private Task OnRedirectToIdentityProviderAsync(RedirectContext context)
        {
            if (!string.IsNullOrEmpty(_azureAdB2COptions.VeracityPlatformServiceUrl))
            {
                context.ProtocolMessage.Scope += $" offline_access {_azureAdB2COptions.VeracityPlatformServiceScopes}";
                context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.Code;
            }
            
            context.ProtocolMessage.RedirectUri = _azureAdB2COptions.RedirectUri;
            return Task.FromResult(0);
        }
    }
}
