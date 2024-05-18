using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace SteamOpenId2._0.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		readonly private IConfiguration _configuration;
		public AuthController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		[HttpGet("Login")]
		public IActionResult Login()
		{
			return Challenge(new AuthenticationProperties() { RedirectUri = _configuration["Steam:Redirect"] }, "Steam");
		}

		[HttpGet("HandleResponse")]
		public async Task<IActionResult> HandleResponse()
		{
			var auth = await HttpContext.AuthenticateAsync();
			var _apiKey = _configuration["Steam:Key"];

			var _steamUserID = auth.Principal.Claims.First(f => f.Type == ClaimTypes.NameIdentifier).Value;

			var steam_request = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={_steamUserID}";

			using (HttpClientHandler httpClientHandler = new HttpClientHandler()
			{
				Proxy = new WebProxy("185.238.228.89", 80),
				UseProxy = true
			})
			{
				using (HttpClient client = new HttpClient())
				{

					await Console.Out.WriteLineAsync(steam_request);
					var result = await client.GetFromJsonAsync<Root>(steam_request);
					var image = await client.GetByteArrayAsync(result.response.players.First().avatarfull);
					var clearid = _steamUserID.Split('/').Last();
					await Console.Out.WriteLineAsync(clearid);


					var query = $"https://steamcommunity.com/inventory/{clearid}/730/2?l=english&count=300";

					await Console.Out.WriteLineAsync(query);
					var items = await client.GetFromJsonAsync<RootItem>(query);
					await Console.Out.WriteLineAsync(JsonSerializer.Serialize(items));
					return File(image, "image/jpeg");
				}
			}

		}
	}
}
