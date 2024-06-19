using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.Core;
using AkouoApi.Models;
using AkouoApi.Services;
using static AkouoApi.Utility.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v2.16.6",
            Title = "APM Akuo API",
            Contact = new OpenApiContact
            {
                Name = "Sara Hentzel",
                Email = "sara_hentzel@sil.org",
            },
        }
    );
});
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(
        policy => {
            policy.WithOrigins("*");
        });
});
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
app.UseCors();

app.MapGet("/hello", (HttpContext httpContext) => {
    ILambdaContext? lambdaContext = httpContext.Items[AbstractAspNetCoreFunction.LAMBDA_CONTEXT] as ILambdaContext;
    lambdaContext?.Logger.LogInformation("Hello from ILambdaContext!");
    return "Hello World!";
});

app.MapGet("/languages", ([FromQuery(Name = "beta")] string? beta,
                           ILogger<Program> _logger, LanguageService _service) =>
        new ApiResponse(_service.GetLanguages(BoolParse(beta)))
).WithName("GetLanguages").Produces<ApiResponse>(200);

app.MapGet("/languages/{iso}", (string iso,
                                [FromQuery(Name = "beta")] string? beta,
                                ILogger<Program> _logger, LanguageService _service) => 
        new ApiResponse(_service.GetLanguage(iso, BoolParse(beta)))
).WithName("GetLanguage").Produces<ApiResponse>(200);

app.MapGet("/bibles", ( [FromQuery(Name = "iso")] string? iso,
                        [FromQuery(Name = "language_code")] string? language_code,
                        [FromQuery(Name = "beta")] string? beta,
                        ILogger<Program> _logger, BibleService _service) => 
        new ApiResponse(language_code == null && iso == null ? 
            _service.GetBibles(BoolParse(beta)) : 
            _service.GetBibleByIso(language_code ?? iso ?? "", BoolParse(beta)))
).WithName("GetBibles").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}", (string bibleId,
                                [FromQuery(Name = "info")] string? info,
                                [FromQuery(Name = "beta")] string? beta,
                                ILogger<Program> _logger, BibleService _service) => 
    new ApiResponse(_service.GetBible(bibleId, BoolParse(beta)))
).WithName("GetBible").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/books", (string bibleId, 
                                       [FromQuery(Name = "beta")] string? beta,
                                       ILogger<Program> _logger, BookService _service) => 
    new ApiResponse(_service.GetBibleBooks(bibleId, BoolParse(beta), null))
).WithName("GetBibleBooks").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}", (string bibleId, 
                                        string book,
                                        [FromQuery(Name = "book_id")] string? book_id,
                                        [FromQuery(Name = "beta")] string? beta,
                                        ILogger<Program> _logger, BookService _service) =>
        new ApiResponse(_service.GetBibleBooks(bibleId, BoolParse(beta), book_id ?? book))                                       
).WithName("GetBibleBook").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/obt_types", (string bibleId,
                                            [FromQuery(Name = "beta")] string? beta,
                                            ILogger<Program> _logger, BibleService _service) =>
                new ApiResponse(_service.GetBibleOBTTypes(bibleId, BoolParse(beta)))
).WithName("GetBibleOBTTypes").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/note_categories", (string bibleId,
                                                [FromQuery(Name = "beta")] string? beta,
                                                ILogger<Program> _logger, BibleService _service) =>
        new ApiResponse(_service.GetBibleNoteCategories(bibleId, BoolParse(beta)))
).WithName("GetBibleNoteCategories").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/movements", (string bibleId,
                                                string book,
                                                [FromQuery(Name = "book_id")] string? book_id,
                                                [FromQuery(Name = "beta")] string? beta,
                                                [FromQuery(Name = "sections")] string? sections,
                                                ILogger<Program> _logger, BookService _service) =>
                new ApiResponse(_service.GetBibleBookMovements(bibleId, book_id ?? book, BoolParse(beta), BoolParse(sections ?? "true")))
).WithName("GetBibleBookMovements").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/movements/{movement}", (string bibleId,
                                                            string book,
                                                            string movement,
                                                            [FromQuery(Name = "book_id")] string? book_id,
                                                            [FromQuery(Name = "beta")] string? beta,
                                                            ILogger<Program> _logger, BookService _service) => 
                new ApiResponse(_service.GetBibleBookMovements(bibleId, book_id ?? book, BoolParse(beta), true, movement))
).WithName("GetBibleBookMovement").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/movements/{movement}/section/{section}", (string bibleId,
                                                                                string book,
                                                                                string movement,
                                                                                string section,
                                                                                [FromQuery(Name = "book_id")] string? book_id,
                                                                                [FromQuery(Name = "beta")] string? beta,
                                                                                ILogger<Program> _logger, BookService _service) => 
                new ApiResponse(_service.GetBibleBookMovements(bibleId, book_id ?? book, BoolParse(beta), true, movement, section))
).WithName("GetBibleBookMovementSection").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/chapters", (string bibleId,
                                                string book,
                                                [FromQuery(Name = "book_id")] string? book_id,
                                                [FromQuery(Name = "beta")] string? beta,
                                                [FromQuery(Name = "sections")] string? sections,
                                                ILogger<Program> _logger, BookService _service) =>
                new ApiResponse(_service.GetBibleBookChapters(bibleId, book_id ?? book, BoolParse(beta), BoolParse(sections??"true")))
).WithName("GetBibleBookChapters").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/chapters/{chapter}", (string bibleId,
                                                           string book,
                                                           string chapter,
                                                           [FromQuery(Name = "book_id")] string? book_id,
                                                           [FromQuery(Name = "beta")] string? beta,
                                                           //[FromQuery(Name = "sections")] string? sections,
ILogger<Program> _logger, BookService _service) =>
                new ApiResponse(_service.GetBibleBookChapters(bibleId, book_id ?? book, BoolParse(beta), true, chapter))
).WithName("GetBibleBookChapter").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/{book}/chapters/{chapter}/section/{section}", 
                                                            (string bibleId,
                                                            string book,
                                                            string chapter,
                                                            string section,
                                                            [FromQuery(Name = "book_id")] string? book_id,
                                                            [FromQuery(Name = "beta")] string? beta,
                                                            ILogger<Program> _logger, BookService _service) =>
                new ApiResponse(_service.GetBibleBookChapters(bibleId, book_id ?? book, BoolParse(beta), true, chapter, section))
).WithName("GetBibleBookChapterSection").Produces<ApiResponse>(200);

app.MapGet("/bibles/{bibleId}/general/books", (string bibleId,
                                       [FromQuery(Name = "beta")] string? beta,
                                       ILogger<Program> _logger, BookService _service) =>
    new ApiResponse(_service.GetBibleBooks(bibleId, BoolParse(beta), null))
).WithName("GetBibleGeneralBooks").Produces<ApiResponse>(200);

app.Run();
