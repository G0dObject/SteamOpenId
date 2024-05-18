using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace SteamOpenId2._0.Controllers
{
	[ApiController]
	public class TestController : ControllerBase
	{
		readonly private IConfiguration _configuration;
		public TestController(IConfiguration configuration)
		{
			_configuration = configuration;
		}


		List<WebProxy> servers = new List<WebProxy>()
		{
			new WebProxy("172.67.182.84", 80),
			new WebProxy("157.25.92.74", 3128),
			new WebProxy("172.67.3.62", 80),
			new WebProxy("45.8.106.145", 80),
			new WebProxy("141.193.213.41", 80),
			new WebProxy("172.64.135.200", 80)
		};



		[HttpGet("/test")]
		public async Task<ContentResult> Index()
		{
			Random rnd = new Random();

			var randomproxy = servers[rnd.Next(servers.Count)];
			return await Request(randomproxy);
		}


		private async Task<ContentResult> Request(WebProxy proxy)
		{
			var _apiKey = _configuration["Steam:Key"];

			var _steamUserID = "https://steamcommunity.com/openid/id/76561198374261740";

			var steam_request = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={_steamUserID}";


			using (HttpClientHandler httpClientHandler = new HttpClientHandler()
			{
				Proxy = proxy,
				UseProxy = true
			})
			{
				using (HttpClient client = new HttpClient())
				{

					await Console.Out.WriteLineAsync(steam_request);
					var result = await client.GetFromJsonAsync<Root>(steam_request);
					string image = result.response.players.First().avatarfull;
					var clearid = _steamUserID.Split('/').Last();
					await Console.Out.WriteLineAsync(clearid);


					var query = $"https://steamcommunity.com/inventory/{clearid}/570/2?l=english&count=300";

					await Console.Out.WriteLineAsync(query);
					var items = await client.GetFromJsonAsync<RootItem>(query);
					await Console.Out.WriteLineAsync(JsonSerializer.Serialize(items));
					return Return(image, items.descriptions);
					//return JsonSerializer.Serialize(items).ToString();
				}
			}
		}
		private ContentResult Return(string image, List<Description> descriptions)
		{
			string list = "";
			foreach (var description in descriptions)
			{
				list += $$""" <li><img src=https://community.akamai.steamstatic.com/economy/image/{{description.icon_url}}></li>""";

			}

			string html = $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Character Profile</title>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #f0f0f0;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                        }
                
                        .profile-container {
                            background-color: #fff;
                            border: 1px solid #ccc;
                            border-radius: 10px;
                            padding: 20px;
                            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            text-align: center;
                        }
                
                        .profile-image img {
                            border-radius: 50%;
                            width: 150px;
                            height: 150px;
                            object-fit: cover;
                            border: 2px solid #4CAF50;
                        }
                
                        .inventory {
                            margin-top: 20px;
                        }
                
                        .inventory h2 {
                            margin-bottom: 10px;
                            color: #333;
                        }
                
                        .inventory ul {
                            list-style-type: none;
                            padding: 0;
                        }
                
                        .inventory ul li {
                            background-color: #f9f9f9;
                            margin: 5px 0;
                            padding: 10px;
                            border: 1px solid #ddd;
                            border-radius: 5px;
                        }
                    </style>
                </head>
                <body>
                    <div class="profile-container">
                        <div class="profile-image">
                            <img src={{image}} alt="">
                        </div>
                        <div class="inventory">
                            <h2>Inventory</h2>
                            <ul>
                            {{list}}
                               
                            </ul>
                        </div>
                    </div>
                </body>
                </html>
                 
                """;
			return new ContentResult
			{
				Content = html,
				ContentType = "text/html"
			};
		}


	}
}



