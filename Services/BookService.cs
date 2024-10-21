using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;


namespace AkouoApi.Services;

public class BookService : BaseService
{
    private long ticks = DateTime.Now.Ticks;
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
    private void WriteLog(string message)
    {
        Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} XXX {DateTime.Now.Ticks - ticks} {message}");
        ticks = DateTime.Now.Ticks;
    }
    private static int [] ReadyChapters(IEnumerable<Published> publishedpassages)
    {
        IEnumerable<int> chapters = publishedpassages.Select(p => p.DestinationChapter()??0);
        return chapters.Where(c => c != 0).ToImmutableSortedSet().ToArray();
    }
    public Book GetBook(List<Published> ready, string book)
    {
        List<Published> myStuff = ready.Where(r => r.Book == book).ToList();
        int [] chapters = ReadyChapters(myStuff);
        int? bookid = myStuff.Select(r => r.Bookid).FirstOrDefault();
        int? altbookid = myStuff.Select(r => r.Altbookid).FirstOrDefault();
        SectionInfo? titleInfo=null;
        SectionInfo? alttitleInfo=null;
        if (bookid != null)
        {
            Section booksection = _context.Sections.Where(s => s.Id == bookid).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
            titleInfo = GetSection(booksection, myStuff);
        }
        if (altbookid != null)
        {
            Section altbooksection = _context.Sections.Where(s => s.Id == altbookid).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
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
    public List<Book> GetBibleBooks(string bibleId, bool scripture, bool beta, string? book)
    {
        Bible? bible = ReadyBibles(beta, bibleId).FirstOrDefault();
        return bible != null ? GetBibleBooks(bible, scripture, beta, book) : throw new Exception("Bible not found");
    }
    private List<Book> GetBibleBooks(Bible bible, bool scripture, bool beta, 
                                     string? book)
    {
        List<Book> books = new();
        List<Published> ready = Ready(scripture, false, beta, bible?.Id, book).ToList();
        
        IEnumerable<string> publishedbooks = ready.Select(p => p.Book ?? "").Distinct();
        publishedbooks.ToList().ForEach(b => {
            books.Add(GetBook(ready, b));
        });
        books.Sort();
        return books;
    }
    public ChapterWrapper GetBibleBookChapters(string bibleId, string bookId, bool scripture, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        ChapterWrapper wrapper = new(bookId);
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null) return wrapper;
        List<ChapterInfo> info = wrapper.Chapters;
        IQueryable<Published> ready = Ready(scripture,false, beta, bible.Id, bookId);
        IQueryable<Published> vernacularq = ready.Where(r => r.Passagetype == null).Include(r => r.Section).ThenInclude(s =>s.TitleMediafile);
        if (justthischapter != null)
            vernacularq = vernacularq.Where(p => p.Startchapter == int.Parse(justthischapter)|| p.Endchapter == int.Parse(justthischapter));
        if (justthissection != null)
            vernacularq = vernacularq.Where(p => p.Sectionid == int.Parse(justthissection));
        List<Published> vernacular = vernacularq.ToList();
        //if there passages that cross chapters, we may have more than one chapter number
        int[] chapters = ReadyChapters(vernacular).Where(c => justthischapter == null || c == int.Parse(justthischapter)).ToArray();

        //Dictionary<Section, IOrderedEnumerable<Section>> allmovements = MovementSections(ReadyMovements(vernacular), vernacular);
        int noteid = NoteType().Id;
        int chapterid = ChapterType().Id;
        foreach (int chapter in chapters)
        {
            List<Published> chapterpsgs = vernacular.Where(p => p.DestinationChapter() == chapter).ToList();
            List<Section> readySections = chapterpsgs.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).OrderBy(x => x.Sequencenum).ToList(); 
            IEnumerable<Section> movements = ReadyMovements(chapterpsgs);
            Published? chapnum = ready.Where(r => r.Book == bookId && r.Passagetype == CHAPTER && r.Reference == CHAPTER+" "+chapter.ToString()).FirstOrDefault();
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
    private SectionInfo GetSection (Section s, List<Published> ready, List<AudioNote>? chapternotes=null,  int chapter=0) 
    {
        NoteLevel level = NoteLevel.Section;
        List<AudioNote> sectionnotes=new();
        List<PassageInfo> passages = new ();
        IOrderedEnumerable<Published> readyPassages = ready.Where(r => r.Sectionid == s.Id).OrderBy(x => x.Sequencenum);
        PassageInfo? curPassage = null;
        bool skipPassage = false;
        bool isPublic = readyPassages.Any(p => p.IsPublic);
        foreach (Published p in readyPassages)
        {
            if (p.Passagetype == null)
            {
                skipPassage = chapter > 0 && p.Startchapter != chapter && p.Endchapter != chapter;
                if (!skipPassage)
                {
                    Mediafile? media = p.Mediafile;
                    curPassage = new PassageInfo(new Passage(p), 
                        p is PublishedScripture ? OBTTypeEnum.scripture : OBTTypeEnum.extra,
                                GetAudio(p.Mediafile), 
                                media?.Transcription);
                    passages.Add(curPassage);
                } else
                    curPassage = null;
                level = NoteLevel.Passage;
            }
            else if (p.Passagetype == CHAPTER) //chapter
            {
                if ( int.TryParse(p.Reference?.Split(" ")[1], out int chnum))
                {
                    p.Startchapter = chnum;
                    p.Endchapter = chnum;
                }
                skipPassage = chapter > 0 && chnum != chapter;
                if (!skipPassage)
                {
                    Mediafile? media = p.Mediafile;
                    curPassage = new PassageInfo(new Passage(p),
                                OBTTypeEnum.chapter,
                                GetAudio(p.Mediafile),
                                media?.Transcription);
                    passages.Add(curPassage);
                }
                else
                    curPassage = null;
                level = NoteLevel.Chapter;
            }
            else if (p.Passagetype == NOTE)
            {
                AudioNote note = new (new Passage(p), OBTTypeEnum.audio_note, GetAudio(p.Mediafile), p.Transcription, p.Sharedresource, GetGraphicImages(p.Passageid, "passage"), GetAudio(p.Sharedresource?.TitleMediafile));
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
        return new SectionInfo(s, GetAudio(s.TitleMediafile), GetGraphicImages(s.Id, "section"), passages.ToArray(), sectionnotes.ToArray(), isPublic);
    }
    public MovementWrapper GetBibleBookMovements(string bibleId, string bookId, bool scripture, bool beta, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetBibleBookMovements");
        MovementWrapper movementWrapper = new (bookId);
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault();
        if (bible == null)
            return movementWrapper;
        List<Published> ready = Ready(scripture,false, beta, bible.Id, bookId).Include(r => r.Titlemediafile).ToList();
        //WriteLog("ready");
        Book book = GetBook(ready, bookId);
        //WriteLog("getbook");
        if (book == null)
            return movementWrapper;
        //Debug.WriteLine("{0} {1}",DateTime.Now.ToLongTimeString(), DateTime.Now.Ticks-ticks);
        movementWrapper.Name = book.Name??"";
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id))
            movementId = id;
        List<Section> movements = ReadyMovements(ready, movementId).ToList();
        //WriteLog("readymovements");


        Dictionary<Section, IOrderedEnumerable<Section>> allmovements = 
            MovementSections(movements, ready);
        //WriteLog("movementsections");
        List<MovementInfo> info = movementWrapper.Movements;
        if (!movements.Any())
        {
            //WriteLog("getsection");
            List<SectionInfo> sectionInfo = new ();
            List<Section> readySections = ready.Where(r => r.Movementid is null && r.Level == SectionLevel.Section)
                .Select(r => new Section(r)).Distinct(new RecordEqualityComparer<Section>())
                .OrderBy(s => s.Sequencenum).ToList();
            if (showSections)
            {
                readySections.ToList().ForEach(s => {
                    if (justthissection == null || s.Id == int.Parse(justthissection))
                    {
                        sectionInfo.Add(GetSection(s, ready));
                    }
                });
            }
            info.Add(new MovementInfo(readySections, null, Array.Empty<Image>(), sectionInfo.ToArray(), ready, 1, null, Array.Empty<AudioNote>()));
        }
        else
        {
            movements.ForEach(m => {
                SectionInfo movementInfo = GetSection(m, ready);
                //WriteLog("getsection");
                List<SectionInfo> sectionInfo = new ();
                if (showSections)
                {
                    allmovements [m].ToList().ForEach(s => {
                        if (justthissection == null || s.Id == int.Parse(justthissection))
                        {
                            sectionInfo.Add(GetSection(s, ready));
                        }
                    });
                }
                info.Add(new MovementInfo(allmovements [m].ToList(), movementInfo.Title_audio.ElementAtOrDefault(0), movementInfo.Images, sectionInfo.ToArray(), ready, Array.IndexOf(allmovements.Keys.ToArray(), m) + 1, m, movementInfo.Audio_notes));
            });
        }
        //WriteLog("done");

        return movementWrapper;
    }
    //return all the sections for the bible/book in one list
    public List<MovementWrapper> GetBibleBookAll(string bibleId, string? bookId, bool scripture, bool beta)
    {
        List<MovementWrapper> all = new ();
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        List<Book> books = GetBibleBooks(bible, scripture, beta, bookId);
        books.ForEach(b => { 
            if (b.Book_id != null)
            {
                MovementWrapper movementWrapper = GetBibleBookMovements(bibleId, b.Book_id, scripture, beta, true);
                all.Add(movementWrapper);
            }
        });
       return all;
    }
}
