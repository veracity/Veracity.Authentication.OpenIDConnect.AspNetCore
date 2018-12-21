using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Veracity.Authentication.OpenIDConnect.Core;

namespace aspnetcoreSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly VeracityPlatformService _veracityPlatformService;
        public HomeController(VeracityPlatformService veracityPlatformService)
        {
            _veracityPlatformService = veracityPlatformService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your identity information.";
            return View();
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
    }
}
