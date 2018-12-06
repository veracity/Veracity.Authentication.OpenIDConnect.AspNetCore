namespace Veracity.Authentication.OpenIDConnect.Core
{
    public class VeracityIntegrationOptions
    {
        public string ClientId { get; set; }
        public string AzureAdB2CInstance { get; set; } = "https://login.microsoftonline.com/tfp";
        public string Tenant { get; set; }
        public string SignUpSignInPolicyId { get; set; }
        public string RedirectUri { get; set; }
        public string DefaultPolicy => SignUpSignInPolicyId;
        public string Authority => $"{AzureAdB2CInstance}/{Tenant}/{DefaultPolicy}/v2.0";
        public string ClientSecret { get; set; }
        public string VeracityPlatformServiceUrl { get; set; }
        public string VeracityPlatformServiceKey { get; set; }
        public string VeracityPlatformServiceScopes { get; set; }
    }
}
