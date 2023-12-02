using AkouoApi.Data;
using AkouoApi.Models;
using AkouoApi.Models;
using AkouoApi.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace AkouoApi.Services;

public class BibleService : BaseService
{

    public BibleService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }

    private List<BibleShort> ShortBibles(List<Bible> bibles)
    {
        List<BibleShort> sb = new();
        
        foreach(Bible b in  bibles)
        { 
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? new Audio[] { audio } : Array.Empty<Audio>();
            sb.Add(new BibleShort(b,aa));
        }
        return sb;
    }
    private List<BibleFull> FullBibles(List<Bible> bibles)
    {
        List<BibleFull> sb = new();

        foreach (Bible b in bibles)
        {
            Audio? audio = GetAudio(b.BibleMediafile);
            Audio[] aa = audio != null ? new Audio[] { audio } : Array.Empty<Audio>();
            sb.Add(new BibleFull(b, aa));
        }
        return sb;
    }

    public List<BibleShort> GetBibles()
    {
        return ShortBibles(ReadyBibles().ToList());
    }
    public List<BibleFull> GetBible(string bibleId)
    {
        return FullBibles(ReadyBibles().Where(o => o.BibleId == bibleId).ToList());
    }
    public List<BibleShort> GetBibleByIso(string iso)
    {
        return ShortBibles(ReadyBibles().Where(o => o.Iso == iso).ToList());
    }
    public List<Book> GetBibleBooks(string bibleId)
    {
        Bible? bible = ReadyBibles().Where(o => o.BibleId == bibleId).FirstOrDefault();
        return GetBibleBooks(bible);
    }
    private List<Book> GetBibleBooks(Bible? bible)
    {
        List<Book> books = new();
        if (bible != null)
        {
            IQueryable<Section> sections = 
                _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
                .Join(_context.Projects, ob => ob.OrganizationId, p => p.OrganizationId, (ob, p) => p)
                .Join(_context.Plans, p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
                .Join(_context.Sections.Where(s => !s.Archived && (s.Published || s.Level < 3)), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile);
            var publishedpassages = sections.Join(_context.Passages.Where(p => !p.Archived), s => s.Id, p => p.SectionId, (s, p) => new {p, s.PlanId });

            IQueryable<string?> publishedbooks = publishedpassages.Select(p => p.p.Book).Distinct();
            //now we have a list of books and we need to look for the book title and alt title
            //these are *probably* in the sections above, unless the book was broken up into
            //multiple plans and the titles are in a plan that doesn't have any published sections  
            publishedbooks.ToList().ForEach(b => {
                Section? title = sections.Join(publishedpassages.Where(pp => pp.p.Book == b),s => s.PlanId, p => p.PlanId, (s, p) => (s))
                    .Where(s => s.Level == 1 && s.Sequencenum == -4).FirstOrDefault();
                Section? alttitle = sections.Join(publishedpassages.Where(pp => pp.p.Book == b),s => s.PlanId, p => p.PlanId, (s, p) => (s))
                    .Where(s => s.Level == 1 && s.Sequencenum == -3).FirstOrDefault();
                IQueryable<Section> thisbooksections = publishedpassages.Where(pp => pp.p.Book == b).Join(sections, p => p.p.SectionId, s=> s.Id, (p, s) => s);
                IQueryable<int> startchapters = publishedpassages.Where(pp => pp.p.Book == b).Select(pp => pp.p.StartChapter ?? 0);
                IQueryable<int> endchapters = publishedpassages.Where(pp => pp.p.Book == b).Select(pp => pp.p.EndChapter ?? 0);
                int [] chapters = startchapters.Union(endchapters).Where(x => x != 0).ToArray();
                List<string> movements = new ();
                thisbooksections.OrderBy(s => s.Sequencenum).ToList().ForEach(s => {
                    Section? movement = sections.Where(m => m.Level == 2 && m.Sequencenum < s.Sequencenum).OrderBy(m => m.Sequencenum).LastOrDefault();
                    if (movement != null)
                        movements.Add(movement.Name);
                });
                if (title == null)
                {
                    var orgpsgwithbook = _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
                                        .Join(_context.Projects, ob => ob.OrganizationId, p => p.OrganizationId, (ob, p) => p)
                                        .Join(_context.Plans, p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
                                        .Join(_context.Sections, pl => pl.Id, s => s.PlanId, (pl, s) => s)
                                        .Join(_context.Passages.Where(p => !p.Archived && p.Book == b), s => s.Id, p => p.SectionId, (s, p) => new {p, s.PlanId });

                    title = sections.Join(orgpsgwithbook, s => s.PlanId, p => p.PlanId, (s, p) => (s))
                                .Where(s => s.Level == 1 && s.Sequencenum == -4).FirstOrDefault();
                    alttitle ??= sections.Join(orgpsgwithbook, s => s.PlanId, p => p.PlanId, (s, p) => (s))
                                .Where(s => s.Level == 1 && s.Sequencenum == -3).FirstOrDefault();
                }
                Audio? audio = title!= null ? GetAudio(title.TitleMediafile) : null;
                Audio? audio_alt = alttitle!= null ? GetAudio(alttitle.TitleMediafile) : null;
                books.Add(new Book()
                {
                    Book_id = b,
                    Name = title?.Name ?? b,
                    Name_long = alttitle?.Name ?? b,
                    Name_alt = alttitle?.Name ?? b,
                    Audio = audio != null ? new Audio[] { audio } : Array.Empty<Audio>(),
                    Audio_alt = audio_alt != null ? new Audio [] { audio_alt } : Array.Empty<Audio>(),
                    //TODO AFTER GRAPHICS REWRITE!! Images = Array.Empty<Image>(),
                    Movements = movements.Distinct().ToArray(),
                    Chapters = chapters
                });
            });
            /*
                .Join(_context.Passages.Where(p => !p.Archived), s => s.Id, p => p.SectionId, (s, p) => p);
                .Select(o => new { o.b.BookId, o.b.BookName, o.b.BookOrder, o.b.BookShortName, o.b.BookTestament, o.b.BookChapters })
                .ToList()
                .ForEach(o => books.Add(new Book(o.BookId, o.BookName, o.BookOrder, o.BookShortName, o.BookTestament, o.BookChapters)))
            */
        }
        return books;
    }

}
