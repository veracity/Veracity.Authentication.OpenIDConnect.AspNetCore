
# Veracity.Authentication.OpenIDConnect.AspNetCore [![NuGet version](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.Core.svg)](https://badge.fury.io/nu/Veracity.Authentication.OpenIDConnect.Core)
Veracity authentication library for dot net core(SDK Version >= 2.1.4)

## For new applications
1. Go to https://developer.veracity.com/ and enroll as developer
2. Create your project and applications using the developer self-service
3. Get  integration information through email which includes client ID etc. 
4. Go to https://developer.veracity.com/doc/create-veracity-app and see the instructions for creating Veracity apps using the Veracity App Generator(https://github.com/veracity/generator-veracity)
5. Update the VeracityIntegration info in the `appsettings.json` file
6. Run the application 

## For existing applications
1. Make sure that your .NET Core version >= 2.1.4. If not, [download the latest version](https://www.microsoft.com/net/download).
2. Go to the NuGet package manager and install `Veracity.Authentication.OpenIDConnect.Core`
3. Put the following code in `Program.cs`
```C#
  public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(s=>s.AddSingleton<IVeracityIntegrationConfigService, VeracityIntegrationConfigService>())
                .ConfigureServices(s=>s.AddSingleton<IVeracityOpenIdManager, VeracityOpenIdManager>())
                .UseStartup<Startup>()
                .Build();
```
4. Put the following code in `Startup.cs`
```C#
 private readonly IVeracityOpenIdManager _veracityOpenIdManager;
        public Startup(IVeracityOpenIdManager veracityOpenIdManager)
        {
            _veracityOpenIdManager = veracityOpenIdManager;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(c => c.LoginPath = new PathString("/account/signin"))
                .AddOpenIdConnect(_veracityOpenIdManager.GetOpenIdOptions());
            services.AddHttpClient<VeracityPlatformService>();
            services.AddMvc();
            services.AddSession();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSession();
            app.UseAuthentication();
        }
```
5. Inject `VeracityPlatformService` into your controller to be able to call the Veracity platform API
```C#
        private readonly VeracityPlatformService _veracityPlatformService;
        public HomeController(VeracityPlatformService veracityPlatformService)
        {
            _veracityPlatformService = veracityPlatformService;
        }
        /// <remarks>
        /// Be aware that the data API and service API have different scopes, this is a matter about whether you can get a valid access token. The service key is also different.
        /// </remarks>
        [Authorize]
        public async Task<IActionResult> CallApiAsync()
        {
            var client = _veracityPlatformService.Client;
            // Calling user related API
            var request = new HttpRequestMessage(HttpMethod.Get, "/platform/my/profile");
            // Calling data fabric API
            // var request = new HttpRequestMessage(HttpMethod.Get, "/datafabric/data/api/1/users/me");
            request.Headers.Authorization = await _veracityPlatformService.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            ViewData["Payload"] = await response.Content.ReadAsStringAsync();
            return View();
        }
```
6. Update the `appsetttings.json` file with information you get after registering your application, go to Veracity support page request subscription key for platform services. 
```json
 "VeracityIntegration": {
    "ClientId": "", 
    "Tenant": "dnvglb2cprod.onmicrosoft.com",
    "SignUpSignInPolicyId": "B2C_1A_SignInWithADFSIdp",
    "RedirectUri": " https://localhost:3000/signin-oidc",
    "ClientSecret": "", 
    "VeracityPlatformServiceUrl": "https://api.veracity.com", 
    "VeracityPlatformServiceKey": "", 
    "VeracityPlatformServiceScopes":
      "https://dnvglb2cprod.onmicrosoft.com/83054ebf-1d7b-43f5-82ad-b2bde84d7b75/user_impersonation"
  },
```
## Integrate with the Veracity policy service (check terms and conditions) and check the service subscription
Veracity will integrate the policy service into identity provider, but before we have done that, you need to check the policy services in your code manually before the user lands on the home page.  
```C#
        [Authorize]
        public async Task<IActionResult> ValidatePolicy()
        {
            var client = _veracityPlatformService.Client;
            var request = new HttpRequestMessage(HttpMethod.Get, "/my/policies/{serviceId}/validate()");
            request.Headers.Authorization = await _veracityPlatformService.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            switch (response.StatusCode)
            {
                    case HttpStatusCode.NoContent:
                        break;
                    case HttpStatusCode.NotAcceptable:
                       var content = await response.Content.ReadAsStringAsync();
                       //you need to grab the url from the respnse and redirect user to this address, Veracity will handle the following stuff. 
                       return Redirect(content.url);
                    default:
                        responseString = $"Error calling API. StatusCode=${response.StatusCode}";
                        break;
            }    
            return View();
        }
```
