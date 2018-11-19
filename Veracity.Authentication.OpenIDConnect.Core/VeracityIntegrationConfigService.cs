using Microsoft.Extensions.Configuration;


namespace Veracity.Authentication.OpenIDConnect.Core
{
    public class VeracityIntegrationConfigService : IVeracityIntegrationConfigService
    {
        private readonly VeracityIntegrationOptions _azureAdB2COptions;
        public VeracityIntegrationConfigService(IConfiguration configuration)
        {
            _azureAdB2COptions = new VeracityIntegrationOptions();
            configuration.GetValue<VeracityIntegrationOptions>("VeracityIntegtaion");
            configuration.GetSection("VeracityIntegtaion").Bind(_azureAdB2COptions);
        }

        public VeracityIntegrationOptions GetVeracityIntegrationConfig()
        {
            return _azureAdB2COptions;
        }
    }

    public interface IVeracityIntegrationConfigService
    {
        VeracityIntegrationOptions GetVeracityIntegrationConfig();
    }
}
