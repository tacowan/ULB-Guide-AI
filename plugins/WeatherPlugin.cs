using System.Net.Http.Headers;
using System.ComponentModel;
using System.Net.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Plugins;
using Microsoft.SemanticKernel.Plugins.Core;

public class WeatherSkill
{

    private readonly string key;
    public WeatherSkill(string key) => this.key = key;
   
    [SKFunction, Description("get the weather forecast for a lat/lon")]
    public async Task<string> GetWeatherAsync(
        [Description("The latitude of the location")] string latlon
    )
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "C# app");

        var url = $"https://api.tomorrow.io/v4/weather/forecast?timesteps=1d&location={latlon}&apikey={key}";   
        var json = await client.GetStringAsync(url);

        return json;
    }



}