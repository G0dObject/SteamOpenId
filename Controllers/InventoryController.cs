using Microsoft.AspNetCore.Mvc;
using SteamOpenId2._0.Views.Inventory;
using System.Net;
using System.Text.Json;

namespace SteamOpenId2._0.Controllers
{
    [Controller]
    public class InventoryController : Controller
    {
        readonly private IConfiguration _configuration;
        public InventoryController(IConfiguration configuration)
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



        [HttpGet("/Inventory")]
        public async Task<IActionResult> InventoryModel()
        {
            Random rnd = new Random();

            var randomproxy = servers[rnd.Next(servers.Count)];
            return await Request(randomproxy);
        }


        private async Task<IActionResult> Request(WebProxy proxy)
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


                    var query = $"https://steamcommunity.com/inventory/{clearid}/730/2?l=english&count=300";

                    await Console.Out.WriteLineAsync(query);
                    var items = await client.GetFromJsonAsync<RootItem>(query);
                    await Console.Out.WriteLineAsync(JsonSerializer.Serialize(items));
                    return Return(image, items.descriptions);
                    //return JsonSerializer.Serialize(items).ToString();
                }
            }
        }
        private IActionResult Return(string image, List<Description> descriptions)
        {

            return base.View(new InventoryViewModel() { Items = descriptions, PlayerImageUrl = image });

        }


    }
}



