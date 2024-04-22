using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AkouoApi.Services;

public class BookService : BaseService
{

    public BookService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }
    private static int [] ReadyChapters(IEnumerable<Passage> publishedpassages)
    {
        IEnumerable<int> startchapters = publishedpassages.Select(p => p.StartChapter ?? 0);
        IEnumerable<int> endchapters = publishedpassages.Select(p => p.EndChapter ?? 0);
        int [] chapters = startchapters.Union(endchapters).Where(x => x != 0).ToArray();
        Array.Sort(chapters);
        return chapters;
    }
    public Book GetBook(Bible bible, string bookId, bool beta)
    {
        IQueryable<Section> titles = 
            _context.OrganizationBibles.Where(o => o.BibleId == bible.Id)
            .Join(_context.Organizations, o => o.OrganizationId, org => org.Id, (o, org) => org)
            .Join(_context.Projects.Where(p => p.Language == bible.Iso), org => org.Id, p => p.OrganizationId, (org, p) => p)
            .Join(_context.Plans, p => p.Id, pl => pl.ProjectId, (p, pl) => pl)
            .Join(_context.Sections.Where(s => !s.Archived && s.Level == 1), pl => pl.Id, s => s.PlanId, (pl, s) => s).Include(s => s.TitleMediafile);

        Section? title = titles.Where(s => s.Sequencenum == -4 && s.State== "BOOK " + bookId).FirstOrDefault();
        Section? alttitle = titles.Where(s => s.Sequencenum == -3 && s.State == "ALTBK " + bookId).FirstOrDefault();
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).Where(p => p.Book == bookId);
        int [] chapters = ReadyChapters(publishedpassages);

        IEnumerable<string> movements = ReadyMovements(bible, bookId, beta, null).Select(s => s.Name);
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
            Title_audio = audio != null ? new Audio [] { audio } : Array.Empty<Audio>(),
            Title_audio_alt = audio_alt != null ? new Audio [] { audio_alt } : Array.Empty<Audio>(),
            Images = graphic,
            Movements = movements.ToArray(),
            Chapters = chapters
        };
    }
    public List<Book> GetBibleBooks(string bibleId, bool beta, string? book)
    {
        Bible? bible = ReadyBibles(beta).Where(o => o.BibleId == bibleId).FirstOrDefault();
        return bible != null ? GetBibleBooks(bible, beta, book) : new List<Book>();
    }
    private List<Book> GetBibleBooks(Bible bible, bool beta, string? book)
    {
        List<Book> books = new();
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).ToList();
        publishedpassages = book != null ? publishedpassages.Where(p => p.Book == book) : publishedpassages.Where(p => (p.Book ?? "") != "");

        IEnumerable<string> publishedbooks = publishedpassages.Select(p => p.Book ?? "").Distinct();
        publishedbooks.ToList().ForEach(b => {
            books.Add(GetBook(bible, b, beta));
        });

        return books;
    }
    public List<ChapterInfo> GetBibleBookChapters(string bibleId, string bookId, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        List<ChapterInfo> info = new ();
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null) return info;
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).Where(p => p.Book == bookId);
        if (justthischapter != null)
            publishedpassages = publishedpassages.Where(p => p.StartChapter == int.Parse(justthischapter)); //|| p.EndChapter == int.Parse(justthischapter));
        IEnumerable<Passage> whichchapters = publishedpassages;
        if (justthissection != null)
            whichchapters = publishedpassages.Where(p => p.Section?.Id == int.Parse(justthissection));
        int [] chapters = ReadyChapters(whichchapters);
        MovementMaps allmovements = MovementMap(bible,bookId);
        List<PublishedAndReady> ready = Ready(false, beta, bible).ToList();
        int noteid = NoteType().Id;
        int chapterid = ChapterType().Id;
        foreach (int chapter in chapters)
        {
            List<Section> readySections = publishedpassages.Where(p => p.StartChapter == chapter).Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).ToList(); //|| p.EndChapter == chapter);
            IEnumerable<Section> movements = MovementsFromSections(readySections, allmovements, null);
            PublishedAndReady? chapnum = ready.Where(r => r.Passage.PassagetypeId == chapterid && r.Passage.Reference == "CHNUM "+chapter.ToString()).FirstOrDefault();
            Audio? audio = chapnum != null ? GetAudio(_mediafileService.GetLatest(chapnum.Passage.Id)) : null;
            Image [] graphics = chapnum != null ?GetGraphicImages(chapnum.Passage.Id, "passage") : Array.Empty<Image>();
            string text = chapnum?.Passage.Title ?? chapter.ToString();
            List<SectionInfo> sectionInfo = new ();
            if (sections)
            {
                foreach (Section s in readySections)
                {
                    List<PassageInfo> passages = new ();
                    IOrderedEnumerable<PublishedAndReady> readyPassages = ready.Where(r => r.Section.Id == s.Id).OrderBy(x => x.Passage.Sequencenum);
                    PassageInfo? curPassage = null;
                    foreach (PublishedAndReady p in readyPassages)
                    {
                        if (p.Passage.PassagetypeId == null)
                        {
                            Mediafile? media = _mediafileService.GetLatest(p.Passage.Id);
                            curPassage = new PassageInfo(p.Passage, GetAudio(_mediafileService.GetLatest(p.Passage.Id)), media?.Transcription);
                            passages.Add(curPassage);

                        }
                        else if (p.Passage.PassagetypeId == noteid)
                        {
                            Sharedresource? sr = GetNoteResource(p.Passage);
                            if (curPassage != null)
                            {
                                Mediafile? media = _mediafileService.GetLatestNote(p.Passage);
                                AudioNote note = new (p.Passage, GetAudio(media), media?.Transcription, sr, GetGraphicImages(p.Passage.Id, "passage"), GetAudio(sr?.TitleMediafile));
                                curPassage.Audio_notes.Add(note);
                            }
                        }
                    };
                    sectionInfo.Add(new SectionInfo(s, GetAudio(s.TitleMediafile), GetGraphicImages(s.Id, "section"), passages.ToArray()));
                }
            }
            info.Add(new ChapterInfo(readySections, audio, graphics, sectionInfo.ToArray(), ready, chapter, text));
        };    
        return info;

    }

    public List<MovementInfo> GetBibleBookMovements(string bibleId, string bookId, bool beta, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        List<MovementInfo> info = new ();
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null)
            return info;
        Dictionary<Section, List<Section>> allmovements = MovementMap(bible, bookId).ByMovement;
        if (allmovements.Count == 0)
            return info;
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id)) movementId = id;
        IEnumerable<Section> movements = ReadyMovements(bible, bookId, beta, movementId);
        if (!movements.Any())
            return info;
        IEnumerable<PublishedAndReady> readyVernacular = VernacularReady(beta, bible).ToList();
        movements.ToList().ForEach(m => {
            List<Section> sections = allmovements[m];

            List<Section> readySections = new ();
            sections.ForEach(s => {
                if (readyVernacular.Any(r => r.Section.Id == s.Id))
                    readySections.Add(s);
            });
            int noteid = NoteType().Id;
            Audio? audio = GetAudio(m.TitleMediafile);
            List<SectionInfo> sectionInfo = new ();
            List<PublishedAndReady> ready = Ready(false, beta, bible).ToList();
            if (showSections)
            {
                readySections.ForEach(s => {
                    if (justthissection == null || s.Id == int.Parse(justthissection))
                    {
                        List<PassageInfo> passages = new ();
                        IOrderedEnumerable<PublishedAndReady> readyPassages = ready.Where(r => r.Section.Id == s.Id).OrderBy(x => x.Passage.Sequencenum);
                        PassageInfo? curPassage = null;
                        foreach (PublishedAndReady p in readyPassages)
                        {
                            if (p.Passage.PassagetypeId == null)
                            {
                                Mediafile? media = _mediafileService.GetLatest(p.Passage.Id);
                                curPassage = new PassageInfo(p.Passage, GetAudio(_mediafileService.GetLatest(p.Passage.Id)), media?.Transcription);
                                passages.Add(curPassage);

                            }
                            else if (p.Passage.PassagetypeId == noteid)
                            {
                                Sharedresource? sr = GetNoteResource(p.Passage);
                                if (curPassage != null)
                                {
                                    Mediafile? media = _mediafileService.GetLatestNote(p.Passage);
                                    AudioNote note = new (p.Passage, GetAudio(media), media?.Transcription, sr, GetGraphicImages(p.Passage.Id, "passage"), GetAudio(sr?.TitleMediafile));
                                    curPassage.Audio_notes.Add(note);
                                }
                            }
                        };
                        sectionInfo.Add(new SectionInfo(s, GetAudio(s.TitleMediafile), GetGraphicImages(s.Id, "section"), passages.ToArray()));
                    }
                });
            }
            info.Add(new MovementInfo(bookId, readySections, audio, GetGraphicImages(m.Id, "section"), sectionInfo.ToArray(), ready, Array.IndexOf(allmovements.Keys.ToArray(), m) + 1, m));
        });
        return info;

    }

}
