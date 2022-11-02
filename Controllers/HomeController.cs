using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using lab03.Models;
using System.Net.Http.Headers;

namespace lab03.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;

    public HomeController(
        ILogger<HomeController> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
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
        ViewBag.message = message;

        var userGetUrl = "https://api.github.com/user";
        _httpClient.DefaultRequestHeaders.Add("Authorization",$"Bearer {message.Access_token}");
        var resp = await _httpClient.GetAsync(userGetUrl);
        var mess = await resp.Content.ReadFromJsonAsync<dynamic>();
        return View();
    }

    public IActionResult Privacy()
    {
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