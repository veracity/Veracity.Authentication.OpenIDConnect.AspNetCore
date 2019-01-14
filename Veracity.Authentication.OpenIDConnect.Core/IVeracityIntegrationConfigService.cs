namespace Veracity.Authentication.OpenIDConnect.Core
{
    public interface IVeracityIntegrationConfigService
    {
        // Get Veracity integration info from app setting
        VeracityIntegrationOptions GetVeracityIntegrationConfig();
    }
}
