using Microsoft.AspNetCore.Mvc;

using System;

using WebApiDemo7.Models;

namespace WebApiDemo7.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }


    [HttpGet("WithAttribute")]
    public ActionResult GetWithAttribute([FromServices] IDateTime dateTime)
                                                        => Ok(dateTime.GetNow());

    [HttpGet("NoAttribute")]
    //https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#parameter-binding-with-di-in-api-controllers
    public ActionResult Get(IDateTime dateTime) => Ok(dateTime.GetNow());


    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
