using BackgroundQueueService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

using System.Text.Json;
using System.Threading.Channels;

using WebApiDemo7.CustomHtmlResult;
using WebApiDemo7.MinimalApiGroup;
using WebApiDemo7.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    //https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#json-property-names-in-validation-errors
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    //https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#parameter-binding-with-di-in-api-controllers
    options.DisableImplicitFromServicesParameters = true;
});


//https://learn.microsoft.com/es-es/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#configure-json-serialization-options
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.IncludeFields = true;
});


builder.Services.AddTransient<IDateTime, DateTimeManager>();








// The max memory to use for the upload endpoint on this instance.
var maxMemory = 500 * 1024 * 1024;

// The max size of a single message, staying below the default LOH size of 85K.
var maxMessageSize = 80 * 1024;

// The max size of the queue based on those restrictions
var maxQueueSize = maxMemory / maxMessageSize;

// Create a channel to send data to the background queue.
builder.Services.AddSingleton<Channel<ReadOnlyMemory<byte>>>((_) =>
                     Channel.CreateBounded<ReadOnlyMemory<byte>>(maxQueueSize));

// Create a background queue service.
builder.Services.AddHostedService<BackgroundQueue>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


//https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#filters-in-minimal-api-apps

string ColorName(string color) => $"Color specified: {color}!";

app.MapGet("/colorSelector/{color}", ColorName)
    .AddEndpointFilter(async (invocationContext, next) =>
    {
        var color = invocationContext.GetArgument<string>(0);

        if (color == "Red")
        {
            return Results.Problem("Red not allowed!");
        }
        return await next(invocationContext);
    });


// Bind to a string array.
// https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#bind-arrays-and-string-values-from-headers-and-query-strings
// GET /tags2?names=john&names=jack&names=jane
app.MapGet("/tags", (string[] names) =>
            $"tag1: {names[0]} , tag2: {names[1]}, tag3: {names[2]}");




//https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#bind-the-request-body-as-a-stream-or-pipereader


//{
//    "Name": "Lautaro",
//    "Age": 23,
//    "Country": "Argentina"
//}


app.MapPost("/StreamQueue", async (HttpRequest req, Stream body,
                                 Channel<ReadOnlyMemory<byte>> queue) =>
{
    if (req.ContentLength is not null && req.ContentLength > maxMessageSize)
    {
        return Results.BadRequest();
    }

    // We're not above the message size and we have a content length, or
    // we're a chunked request and we're going to read up to the maxMessageSize + 1. 
    // We add one to the message size so that we can detect when a chunked request body
    // is bigger than our configured max.
    var readSize = (int?)req.ContentLength ?? (maxMessageSize + 1);

    var buffer = new byte[readSize];

    // Read at least that many bytes from the body.
    var read = await body.ReadAtLeastAsync(buffer, readSize, throwOnEndOfStream: false);

    // We read more than the max, so this is a bad request.
    if (read > maxMessageSize)
    {
        return Results.BadRequest();
    }

    // Attempt to send the buffer to the background queue.
    if (queue.Writer.TryWrite(buffer.AsMemory(0..read)))
    {
        return Results.Accepted();
    }

    // We couldn't accept the message since we're overloaded.
    return Results.StatusCode(StatusCodes.Status429TooManyRequests);
});



//https://learn.microsoft.com/es-es/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#customizing-responses
app.MapGet("/html", () => {

    return Results.Extensions.Html(@$"<!doctype html>
        <html>
            <head><title>miniHTML</title></head>
            <body>
                <h1>Hello World</h1>
                <p>The time on the server is {DateTime.Now:O}</p>
            </body>
        </html>");
    }
);


https://learn.microsoft.com/es-es/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-7.0#configure-json-serialization-options

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

app.MapGet("/GetJsonPerson", () => Results.Json(new Person
              {
                  Name = "Walk dog",
                  Age = 23,
                  Country= "Argentina"
              }, options));


//https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#openapi-improvements-for-minimal-apis
app.MapPost("/persons/{id}", async (int id, Person person) =>
{
 
    return Results.Created($"/todoitems/{id}", person);
})
.WithOpenApi(generatedOperation =>
{
    var parameter = generatedOperation.Parameters[0];
    parameter.Description = "The ID associated with the created Todo";
    return generatedOperation;
});


// https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#file-uploads-using-iformfile-and-iformfilecollection
app.MapPost("/upload", async (IFormFile file) =>
{
    var tempFile = Path.GetTempFileName();
    app.Logger.LogInformation(tempFile);
    using var stream = File.OpenWrite(tempFile);
    await file.CopyToAsync(stream);
});

//https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#file-uploads-using-iformfile-and-iformfilecollection

app.MapPost("/upload_many", async (IFormFileCollection myFiles) =>
{
    foreach (var file in myFiles)
    {
        var tempFile = Path.GetTempFileName();
        app.Logger.LogInformation(tempFile);
        using var stream = File.OpenWrite(tempFile);
        await file.CopyToAsync(stream);
    }
});


//https://learn.microsoft.com/es-es/aspnet/core/fundamentals/minimal-apis?preserve-view=true&view=aspnetcore-7.0#parameter-binding-for-argument-lists-with-asparameters

app.MapGet("/ap/todoitems/{id}", ([AsParameters] Person request) =>
{
    return Results.Ok(request);
});



// https://learn.microsoft.com/es-es/aspnet/core/release-notes/aspnetcore-7.0?view=aspnetcore-7.0#route-groups

app.MapGroup("/public/todos")
    .MapTodosApi()
    .AddEndpointFilter((context, next) =>
    {
        app.Logger.LogInformation("/group filter");
        return next(context);
    })
    .WithTags("Public");



app.Run();
