using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using lab03.Models;
using System.Net.Http.Headers;
using System.Net;

namespace lab03.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _clientFactory;

    public HomeController(
        ILogger<HomeController> logger,
        HttpClient httpClient,
        IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _httpClient = httpClient;
        _clientFactory = clientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Authorize([FromQuery] string code) 
    {
        var param = new {
            code=code,
            client_id="9b088fd1b8e53b515c05",
            client_secret="fc08b57954763fb0e9abdb5d25d8391fd79d34d3"
        };
        var url = "https://github.com/login/oauth/access_token";
        _httpClient.DefaultRequestHeaders.Add("accept","application/json");
        var response = await _httpClient.PostAsJsonAsync(url,param);
        var message = await response.Content.ReadFromJsonAsync<AuthResponse>();
        HttpContext.Session.SetString("Token",message.Access_token);
        ViewBag.message = message;
        return RedirectToAction("Profile");
    }

    public async Task<IActionResult> Profile()
    {
        var token = HttpContext.Session.GetString("Token");
        if(token is null) return RedirectToAction("Error");
        var userGetUrl = "https://api.github.com/user";
        var request = new HttpRequestMessage(HttpMethod.Get,userGetUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",HttpContext.Session.GetString("Token"));
        request.Headers.UserAgent.TryParseAdd("request");
        
        var client = _clientFactory.CreateClient();
        var resp = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if(resp.StatusCode == HttpStatusCode.OK) {
            var apiString = await resp.Content.ReadAsStringAsync();
            ViewBag.scope = apiString;
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class AuthResponse {
    public string Access_token {get;set;}
    public string Scope {get;set;}
    public string Token_type {get;set;}
}