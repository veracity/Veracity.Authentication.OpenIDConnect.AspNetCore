using Microsoft.Extensions.Configuration;

namespace Veracity.Authentication.OpenIDConnect.Core
{
    public class VeracityIntegrationConfigService : IVeracityIntegrationConfigService
    {
        public const string SectionKey = "VeracityIntegration";

        private readonly IConfiguration configuration;

        private VeracityIntegrationOptions azureAdB2COptions;

        public VeracityIntegrationConfigService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public virtual VeracityIntegrationOptions GetVeracityIntegrationConfig()
        {
            if (this.azureAdB2COptions != null)
            {
                return this.azureAdB2COptions;
            }

            this.azureAdB2COptions = new VeracityIntegrationOptions();
            this.configuration.GetValue<VeracityIntegrationOptions>(SectionKey);
            this.configuration.GetSection(SectionKey).Bind(this.azureAdB2COptions);
            return this.azureAdB2COptions;
        }
    }

}
