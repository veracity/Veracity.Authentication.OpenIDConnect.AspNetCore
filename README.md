# Veracity.Authentication.OpenIDConnect.Core
Veracity authentication connector for dot net core(SDK Version >= 2.1.4)

## For new application
1. Go to https://developer.veracity.com/ enroll as developer
2. Create you project and applicaitons using developer self-service
3. Get integration information through email which include client id etc. 
4. Go to https://developer.veracity.com/doc/create-veracity-app find the instructions how to create veracity app using Veracity App Generator(https://github.com/veracity/generator-veracity)
5. Copy the application integration info to app.setttings
6. Run the application 

## For existed application
1. Make sure the dot net core version >= 2.1.4, go to https://www.microsoft.com/net/download download and install the latest version
2. Go to Nuget package manager find "Veracity.Authentication.OpenIDConnect.Core" and install it
3. Put following code in *Program.cs*
```C#
  public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(s=>s.AddSingleton<IVeracityIntegrationConfigService, VeracityIntegrationConfigService>())
                .ConfigureServices(s=>s.AddSingleton<IVeracityOpenIdManager,VeracityOpenIdManager>())
                .UseStartup<Startup>()
                .Build();
```
4. Put following code in *Startup.cs*
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
5. Inject VeracityPlatformService into your controller to call Veracity platform API
```C#
        private readonly VeracityPlatformService _veracityPlatformService;
        public HomeController(VeracityPlatformService veracityPlatformService)
        {
            _veracityPlatformService = veracityPlatformService;
        }
        [Authorize]
        public async Task<IActionResult> CallApiAsync()
        {
            var client = _veracityPlatformService.Client;
            var request = new HttpRequestMessage(HttpMethod.Get, "/platform/my/profile");
            request.Headers.Authorization = await _veracityPlatformService.GetAuthenticationHeaderAsync();
            var response = await client.SendAsync(request);
            ViewData["Payload"] = await response.Content.ReadAsStringAsync();
            return View();
        }
```
## Integrate with Veracity policy service(check terms and conditions) and check the service subscription
Veracity will integrate the policy service into identity provider, but before we have done that, you need to check the policy services in your code mannully and before the user landing to home page.  
```C#
        private readonly VeracityPlatformService _veracityPlatformService;
        public HomeController(VeracityPlatformService veracityPlatformService)
        {
            _veracityPlatformService = veracityPlatformService;
        }
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
