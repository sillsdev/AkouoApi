using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
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
    public BookService(ILogger<LanguageService> logger,
                           AppDbContext context,
                           IS3Service s3Service,
                           MediafileService mediafileService) : base(logger, context, s3Service, mediafileService)
    {
    }
    private static int [] ReadyChapters(IEnumerable<PublishedScripture> publishedpassages)
    {
        IEnumerable<int> startchapters = publishedpassages.Select(p => p.Startchapter ?? 0);
        IEnumerable<int> endchapters = publishedpassages.Select(p => p.Endchapter ?? 0);
        int [] chapters = startchapters.Union(endchapters).Where(x => x != 0).ToArray();
        Array.Sort(chapters);
        return chapters;
    }
    public Book GetBook(List<PublishedScripture> ready, string book)
    {
        List<PublishedScripture> myStuff = ready.Where(r => r.Book == book).ToList();
        int [] chapters = ReadyChapters(myStuff);
        int? bookid = myStuff.Select(r => r.Bookid).FirstOrDefault();
        int? altbookid = myStuff.Select(r => r.Altbookid).FirstOrDefault();
        SectionInfo? titleInfo=null;
        SectionInfo? alttitleInfo=null;
        if (bookid != null)
        {
            Section booksection = _context.Sections.Where(s => s.Id == altbookid).FirstOrDefault() ?? new Section();
            titleInfo = GetSection(booksection, myStuff);
        }
        if (altbookid != null)
        {
            Section altbooksection = _context.Sections.Where(s => s.Id == bookid).FirstOrDefault() ?? new Section();
            alttitleInfo = GetSection(altbooksection, myStuff);
        }
        IEnumerable<string> movements = ReadyMovements(myStuff).Select(s => s.Name);
        return new Book()
        {
            Id = bookid??0,
            Book_id = book,
            Name = titleInfo?.Text ?? book,
            Name_long = alttitleInfo?.Text ?? titleInfo?.Text ?? book,
            Name_alt = alttitleInfo?.Text ?? book,
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
        Bible? bible = ReadyBibles(beta, bibleId).FirstOrDefault();
        return bible != null ? GetBibleBooks(bible, beta, book) : new List<Book>();
    }
    private List<Book> GetBibleBooks(Bible bible, bool beta, 
                                     string? book)
    {
        List<Book> books = new();
        List<PublishedScripture> ready = Ready(false, beta, bible, book).ToList();
        
        IEnumerable<string> publishedbooks = ready.Where(r => r.Passagetype == null).Select(p => p.Book ?? "").Distinct();
        publishedbooks.ToList().ForEach(b => {
            books.Add(GetBook(ready, b));
        });

        return books;
    }
    public ChapterWrapper GetBibleBookChapters(string bibleId, string bookId, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        ChapterWrapper wrapper = new(bookId);
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null) return wrapper;
        List<ChapterInfo> info = wrapper.Chapters;
        IQueryable<PublishedScripture> ready = Ready(false, beta, bible, bookId);
        IQueryable<PublishedScripture> vernacularq = ready.Where(r => r.Passagetype == null);
        if (justthischapter != null)
            vernacularq = vernacularq.Where(p => p.Startchapter == int.Parse(justthischapter)|| p.Endchapter == int.Parse(justthischapter));
        if (justthissection != null)
            vernacularq = vernacularq.Where(p => p.Sectionid == int.Parse(justthissection));
        List<PublishedScripture> vernacular = vernacularq.ToList();
        //if there passages that cross chapters, we may have more than one chapter number
        int[] chapters = ReadyChapters(vernacular).Where(c => justthischapter == null || c == int.Parse(justthischapter)).ToArray();

        //Dictionary<Section, IOrderedEnumerable<Section>> allmovements = MovementSections(ReadyMovements(vernacular), vernacular);
        int noteid = NoteType().Id;
        int chapterid = ChapterType().Id;
        foreach (int chapter in chapters)
        {
            List<PublishedScripture> chapterpsgs = vernacular.Where(p => p.Startchapter == chapter||p.Endchapter==chapter).ToList();
            List<Section> readySections = chapterpsgs.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).OrderBy(x => x.Sequencenum).ToList(); 
            IEnumerable<Section> movements = ReadyMovements(chapterpsgs);
            PublishedScripture? chapnum = ready.Where(r => r.Book == bookId && r.Passagetype == CHAPTER && r.Reference == CHAPTER+" "+chapter.ToString()).FirstOrDefault();
            Audio? audio = chapnum != null ? GetAudio(_mediafileService.GetLatest(chapnum.Passageid)) : null;
            Image [] graphics = chapnum != null ?GetGraphicImages(chapnum.Passageid, "passage") : Array.Empty<Image>();
            string text = chapnum?.Title ?? chapter.ToString();
            List<SectionInfo> sectionInfo = new ();
            List<AudioNote> chapternotes = new();
            if (sections)
            {
                foreach (Section s in readySections)
                {
                    sectionInfo.Add(GetSection(s, ready.ToList(), chapternotes, chapter));
                }
            }
            info.Add(new ChapterInfo(readySections, chapterpsgs.OrderBy(x => x.Sequencenum), movements, audio, graphics, sectionInfo.ToArray(), chapternotes, ready, chapter, text));
        };    
        return wrapper;
    }
    private SectionInfo GetSection (Section s, List<PublishedScripture> ready, List<AudioNote>? chapternotes=null,  int chapter=0) 
    {
        NoteLevel level = NoteLevel.Section;
        List<AudioNote> sectionnotes=new();
        List<PassageInfo> passages = new ();
        IOrderedEnumerable<PublishedScripture> readyPassages = ready.Where(r => r.Sectionid == s.Id).OrderBy(x => x.Sequencenum);
        PassageInfo? curPassage = null;
        bool skipPassage = false;
        foreach (PublishedScripture p in readyPassages)
        {
            if (p.Passagetype == null)
            {
                skipPassage = chapter > 0 && p.Startchapter != chapter && p.Endchapter != chapter;
                if (!skipPassage)
                {
                    Mediafile? media = p.Mediafile;
                    curPassage = new PassageInfo(new Passage(p), 
                                GetAudio(p.Mediafile), 
                                media?.Transcription);
                    passages.Add(curPassage);
                } else
                    curPassage = null;
                level = NoteLevel.Passage;
            }
            else if (p.Passagetype == CHAPTER) //chapter
            {
                level = NoteLevel.Chapter;
            }
            else if (p.Passagetype == NOTE)
            {
                AudioNote note = new (new Passage(p), GetAudio(p.Mediafile), p.Transcription, p.Sharedresource, GetGraphicImages(p.Passageid, "passage"), GetAudio(p.Sharedresource?.TitleMediafile));
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
        List<PublishedScripture> ready = Ready(false, beta, bible, bookId).Include(r => r.Titlemediafile).ToList();
        Book book = GetBook(ready, bookId);
        if (book == null)
            return movementWrapper;
        movementWrapper.Name = book.Name??"";
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id))
            movementId = id;
        List<Section> movements = ReadyMovements(ready, movementId).ToList();

        if (!movements.Any())
            return movementWrapper;
        Dictionary<Section, IOrderedEnumerable<Section>> allmovements = 
            MovementSections(movements, ready);
        List<MovementInfo> info = movementWrapper.Movements;
        movements.ForEach(m => {
            SectionInfo movementInfo = GetSection(m, ready);
            List<SectionInfo> sectionInfo = new ();
            if (showSections)
            {
                allmovements[m].ToList().ForEach(s => {
                    if (justthissection == null || s.Id == int.Parse(justthissection))
                    {
                        sectionInfo.Add(GetSection(s, ready));
                    }
                });
            }
            info.Add(new MovementInfo(allmovements[m].ToList(), movementInfo.Title_audio.ElementAtOrDefault(0), movementInfo.Images, sectionInfo.ToArray(), ready, Array.IndexOf(allmovements.Keys.ToArray(), m) + 1, m, movementInfo.Audio_notes));
        });
        return movementWrapper;
    }

}
