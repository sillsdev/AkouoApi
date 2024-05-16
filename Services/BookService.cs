using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace AkouoApi.Services;

public class BookService : BaseService
{
    private enum NoteLevel: int
    {
        Book = 1,
        Movement = 2,
        Chapter = 3,
        Section = 4,
        Passage = 5
    }
    private readonly int Noteid;
    private readonly int Chapterid;
    public BookService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
        Noteid = NoteType().Id;
        Chapterid = ChapterType().Id;
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
        List<PublishedAndReady> ready = Ready(false, beta, bible).ToList();
        SectionInfo? titleInfo = title == null ? null : GetSection(title, ready);
        Section? alttitle = titles.Where(s => s.Sequencenum == -3 && s.State == "ALTBK " + bookId).FirstOrDefault();
        SectionInfo? alttitleInfo = alttitle == null ? null : GetSection(alttitle, ready);
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).Where(p => p.Book == bookId);
        int [] chapters = ReadyChapters(publishedpassages);

        IEnumerable<string> movements = ReadyMovements(bible, bookId, beta, null).Select(s => s.Name);
         return new Book()
        {
            Book_id = bookId,
            Name = title?.Name ?? bookId,
            Name_long = alttitle?.Name ?? bookId,
            Name_alt = alttitle?.Name ?? bookId,
            Title_audio = titleInfo?.Title_audio ?? Array.Empty<Audio>(),
            Title_audio_alt = alttitleInfo?.Title_audio ?? Array.Empty<Audio>(),
            Images = titleInfo?.Images ?? Array.Empty<Image>(),
            Movements = movements.ToArray(),
            Chapters = chapters,
            Audio_notes = titleInfo?.Audio_notes ?? Array.Empty<AudioNote>(),
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
    public ChapterWrapper GetBibleBookChapters(string bibleId, string bookId, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        ChapterWrapper wrapper = new(bookId);
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null) return wrapper;
        List<ChapterInfo> info = wrapper.Chapters;
        IEnumerable<Passage> publishedpassages = ReadyVernacularPassages(bible, beta).Where(p => p.Book == bookId);
        if (justthischapter != null)
            publishedpassages = publishedpassages.Where(p => p.StartChapter == int.Parse(justthischapter)|| p.EndChapter == int.Parse(justthischapter));
        if (justthissection != null)
            publishedpassages = publishedpassages.Where(p => p.Section?.Id == int.Parse(justthissection));
        //if there passages that cross chapters, we may have more than one chapter number
        int[] chapters = ReadyChapters(publishedpassages).Where(c => justthischapter == null || c == int.Parse(justthischapter)).ToArray();

        MovementMaps allmovements = MovementMap(bible,bookId);
        List<PublishedAndReady> ready = Ready(false, beta, bible).ToList();
        int noteid = NoteType().Id;
        int chapterid = ChapterType().Id;
        foreach (int chapter in chapters)
        {
            IEnumerable<Passage> chapterpsgs = publishedpassages.Where(p => p.StartChapter == chapter||p.EndChapter==chapter);
            List<Section> readySections = chapterpsgs.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).OrderBy(x => x.Sequencenum).ToList(); 
            IEnumerable<Section> movements = MovementsFromSections(readySections, allmovements, null);
            PublishedAndReady? chapnum = ready.Where(r => r.Passage.Book == bookId && r.Passage.PassagetypeId == chapterid && r.Passage.Reference == "CHNUM "+chapter.ToString()).FirstOrDefault();
            Audio? audio = chapnum != null ? GetAudio(_mediafileService.GetLatest(chapnum.Passage.Id)) : null;
            Image [] graphics = chapnum != null ?GetGraphicImages(chapnum.Passage.Id, "passage") : Array.Empty<Image>();
            string text = chapnum?.Passage.Title ?? chapter.ToString();
            List<SectionInfo> sectionInfo = new ();
            List<AudioNote> chapternotes = new();
            if (sections)
            {
                foreach (Section s in readySections)
                {
                    sectionInfo.Add(GetSection(s, ready, chapternotes, chapter));
                }
            }
            info.Add(new ChapterInfo(readySections, chapterpsgs.OrderBy(x => x.Sequencenum), movements, audio, graphics, sectionInfo.ToArray(), chapternotes, ready, chapter, text));
        };    
        return wrapper;
    }
    private SectionInfo GetSection (Section s, List<PublishedAndReady> ready, List<AudioNote>? chapternotes=null,  int chapter=0) 
    {
        NoteLevel level = NoteLevel.Section;
        List<AudioNote> sectionnotes=new();
        List<PassageInfo> passages = new ();
        IOrderedEnumerable<PublishedAndReady> readyPassages = ready.Where(r => r.Section.Id == s.Id).OrderBy(x => x.Passage.Sequencenum);
        PassageInfo? curPassage = null;
        bool skipPassage = false;
        foreach (PublishedAndReady p in readyPassages)
        {
            if (p.Passage.PassagetypeId == null)
            {
                skipPassage = chapter > 0 && p.Passage.StartChapter != chapter && p.Passage.EndChapter != chapter;
                if (!skipPassage)
                {
                    Mediafile? media = _mediafileService.GetLatest(p.Passage.Id);
                    curPassage = new PassageInfo(p.Passage, GetAudio(_mediafileService.GetLatest(p.Passage.Id)), media?.Transcription);
                    passages.Add(curPassage);
                } else
                    curPassage = null;
                level = NoteLevel.Passage;
            }
            else if (p.Passage.PassagetypeId == Chapterid) //chapter
            {
                level = NoteLevel.Chapter;
            }
            else if (p.Passage.PassagetypeId == Noteid)
            {
                Sharedresource? sr = GetNoteResource(p.Passage);
                Mediafile? media = _mediafileService.GetLatestNote(p.Passage);
                AudioNote note = new (p.Passage, GetAudio(media), media?.Transcription, sr, GetGraphicImages(p.Passage.Id, "passage"), GetAudio(sr?.TitleMediafile));
                if (level == NoteLevel.Chapter && chapternotes != null)
                {
                    chapternotes.Add(note);
                } else if (level == NoteLevel.Section)
                {
                    sectionnotes.Add(note);
                }
                else if (level == NoteLevel.Passage && curPassage != null) //passage note
                {
                    curPassage.Audio_notes.Add(note);
                }

            }
        };
        return new SectionInfo(s, GetAudio(s.TitleMediafile), GetGraphicImages(s.Id, "section"), passages.ToArray(), sectionnotes.ToArray());
    }
    public MovementWrapper GetBibleBookMovements(string bibleId, string bookId, bool beta, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        MovementWrapper movementWrapper = new (bookId);
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null)
            return movementWrapper;
        Book book = GetBook(bible, bookId, beta);
        if (book == null)
            return movementWrapper;
        movementWrapper.Name = book.Name??"";
        Dictionary<Section, List<Section>> allmovements = MovementMap(bible, bookId).ByMovement;
        if (allmovements.Count == 0)
            return movementWrapper;
        List<MovementInfo> info = movementWrapper.Movements;
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id)) movementId = id;
        IEnumerable<Section> movements = ReadyMovements(bible, bookId, beta, movementId);
        if (!movements.Any())
            return movementWrapper;
        IEnumerable<PublishedAndReady> readyVernacular = VernacularReady(beta, bible).ToList();
        movements.ToList().ForEach(m => {
            List<Section> sections = allmovements[m];
            List<Section> readySections = new ();
            sections.ForEach(s => {
                if (readyVernacular.Any(r => r.Section.Id == s.Id))
                    readySections.Add(s);
            });
            List<PublishedAndReady> ready = Ready(false, beta, bible).ToList();
            SectionInfo movementInfo = GetSection(m, ready);
            List<SectionInfo> sectionInfo = new ();
            if (showSections)
            {
                readySections.ForEach(s => {
                    if (justthissection == null || s.Id == int.Parse(justthissection))
                    {
                        sectionInfo.Add(GetSection(s, ready));
                    }
                });
            }
            info.Add(new MovementInfo(readySections, movementInfo.Title_audio.ElementAtOrDefault(0), movementInfo.Images, sectionInfo.ToArray(), ready, Array.IndexOf(allmovements.Keys.ToArray(), m) + 1, m, movementInfo.Audio_notes));
        });
        return movementWrapper;

    }

}
