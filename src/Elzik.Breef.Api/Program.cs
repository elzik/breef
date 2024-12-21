using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();
app.UseCors();

var breefs = app.MapGroup("/breefs");
breefs.MapPost("/", async (Breef breef) =>
{
    Debug.WriteLine(DateTime.Now.TimeOfDay.TotalNanoseconds + ": " + breef.Url);

    return Results.Created(breef.Url, breef);
});

app.Run();

public record Breef(string Url);

[JsonSerializable(typeof(Breef))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
