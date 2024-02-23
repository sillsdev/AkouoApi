using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using AkouoApi.Models;
using AkouoApi.Services;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiServices();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/hello", (HttpContext httpContext) => {
    ILambdaContext? lambdaContext = httpContext.Items[AbstractAspNetCoreFunction.LAMBDA_CONTEXT] as ILambdaContext;
    lambdaContext?.Logger.LogInformation("Hello from ILambdaContext!");
    return "Hello World!";
});

app.MapGet("/languages", (ILogger<Program> _logger, LanguageService _service) =>
    new ApiResponse (_service.GetLanguages())
).WithName("GetLanguages").Produces<ApiResponse>(200);

app.MapGet("/languages/{iso}", (string iso, 
                                    ILogger<Program> _logger, LanguageService _service) => 
    new ApiResponse(_service.GetLanguage(iso))
).WithName("GetLanguage").Produces<ApiResponse>(200);

app.MapGet("/bibles", (
    [FromQuery(Name = "iso")] string? iso,
    [FromQuery(Name = "language_code")] string? language_code, 
    ILogger<Program> _logger, BibleService _service) =>
    new ApiResponse(language_code == null && iso == null ? _service.GetBibles() : _service.GetBibleByIso(language_code??iso??""))
).WithName("GetBibles").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}", (string bibleId,
    [FromQuery(Name = "info")] string? info,
                                    ILogger<Program> _logger, BibleService _service) => 
    new ApiResponse(_service.GetBible(bibleId))
).WithName("GetBible").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/books", (string bibleId, 
                                            ILogger<Program> _logger, BookService _service) =>
    new ApiResponse(_service.GetBibleBooks(bibleId, null))
).WithName("GetBibleBooks").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}", (string bibleId, 
                                        string book,
                                        [FromQuery(Name = "book_id")] string? book_id,
                                            ILogger<Program> _logger, BookService _service) =>
                new ApiResponse(_service.GetBibleBooks(bibleId, book_id ?? book))
                                               
).WithName("GetBibleBook").Produces<ApiResponse>(200);
app.MapGet("/bibles/{bibleId}/obt_types", (string bibleId,
                                           ILogger<Program> _logger, OBTTypeService _service) =>
                new ApiResponse(_service.GetBibleOBTTypes(bibleId))

).WithName("GetBibleOBTTypes").Produces<ApiResponse>(200);

app.Run();
