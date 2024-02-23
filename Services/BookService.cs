using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace AkouoApi.Services;

public class BookService : BaseService
{

    public BookService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }
    public Book GetBook(Bible bible, string bookId)
    {
        IQueryable<Section> titles = 
            _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
            .Join(_context.Organizations, o => o.OrganizationId, org => org.Id, (o, org) => org)
            .Join(_context.Projects.Where(p => p.Language == bible.Iso), org => org.Id, p => p.OrganizationId, (org, p) => p)
            .Join(_context.Plans, p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
            .Join(_context.Sections.Where(s => !s.Archived && s.Level == 1), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile);

        Section? title = titles.Where(s => s.Sequencenum == -4 && s.State== "BOOK " + bookId).FirstOrDefault();
        Section? alttitle = titles.Where(s => s.Sequencenum == -3 && s.State == "ALTBK " + bookId).FirstOrDefault();
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible).Where(p => p.Book == bookId);
        IEnumerable<int> startchapters = publishedpassages.Select(p => p.StartChapter ?? 0);
        IEnumerable<int> endchapters = publishedpassages.Select(p => p.EndChapter ?? 0);
        int [] chapters = startchapters.Union(endchapters).Where(x => x != 0).ToArray();

        IEnumerable<string> movements = ReadyMovements(bible, bookId).Select(s => s.Name);
        Audio? audio = title != null ? GetAudio(title.TitleMediafile) : null;
        Image [] graphic = title != null ? GetGraphicImages(title.Id, "section") : Array.Empty<Image>();
        Audio? audio_alt = alttitle != null ? GetAudio(alttitle.TitleMediafile) : null;
        //Image []? graphic_alt = alttitle != null ? GetGraphicImages(alttitle.Id, "section") : Array.Empty<Image>();
        return new Book()
        {
            Book_id = bookId,
            Name = title?.Name ?? bookId,
            Name_long = alttitle?.Name ?? bookId,
            Name_alt = alttitle?.Name ?? bookId,
            Audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>(),
            Audio_alt = audio_alt != null ? new Audio [] { audio_alt } : Array.Empty<Audio>(),
            Images = graphic,
            Movements = movements.ToArray(),
            Chapters = chapters
        };
    }
    public List<Book> GetBibleBooks(string bibleId, string? book)
    {
        Bible? bible = ReadyBibles().Where(o => o.BibleId == bibleId).FirstOrDefault();
        return bible != null ? GetBibleBooks(bible, book) : new List<Book>();
    }
    private List<Book> GetBibleBooks(Bible bible, string? book)
    {
        List<Book> books = new();
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible).ToList();
        publishedpassages = book != null ? publishedpassages.Where(p => p.Book == book) : publishedpassages.Where(p => (p.Book ?? "") != "");

        IEnumerable<string> publishedbooks = publishedpassages.Select(p => p.Book ?? "").Distinct();
        publishedbooks.ToList().ForEach(b => {
            books.Add(GetBook(bible, b));
        });

        return books;
    }
}
