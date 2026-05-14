using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore;



namespace AkouoApi.Services;

public class BookService(ILogger<LanguageService> logger,
                       AppDbContext context,
                       IS3Service s3Service,
                       MediafileService mediafileService) : BaseService(logger, context, s3Service, mediafileService)
{
    //private long ticks = DateTime.Now.Ticks;
    private enum NoteLevel : int
    {
        Book = 1,
        Movement = 2,
        Chapter = 3,
        Section = 4,
        Passage = 5
    }
    /*
    private void WriteLog(string message)
    {
        Debug.WriteLine($"{DateTime.Now.ToLongTimeString()} XXX {DateTime.Now.Ticks - ticks} {message}");
        ticks = DateTime.Now.Ticks;
    }
    */
    private static ChapterShort [] ReadyChapters(bool sections, IEnumerable<Published> publishedpassages, IEnumerable<PublishedChapter> publishedchapters, string bookId)
    {
        List<ChapterShort> ret = [];
        IEnumerable<int> chapternums = publishedpassages.Select(p => p.DestinationChapter()??0).Distinct();
        List<int> chapters = [.. chapternums.Where(c => c != 0).OrderBy(c => c)];

        IEnumerable<PublishedChapter> pubchapters = publishedchapters.Where(ch => ch.Book == bookId);

        chapters.ForEach(c => {
            Published? chapnum = publishedpassages.Where(r => r.Passagetype == CHAPTER && r.Reference == CHAPTER+" "+c.ToString()).FirstOrDefault();
            List<SectionShort> sectionInfo = [];
            if (sections)
            {
                List<Published> chapterpsgs = publishedpassages.Where(p => p.DestinationChapter() == c).ToList();
                List<Section> readySections = [.. chapterpsgs.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).OrderBy(x => x.Sequencenum)];

                List<AudioNote> chapternotes = [];
                foreach (Section s in readySections)
                {
                    sectionInfo.Add(new SectionShort(s));
                }
            }
            IEnumerable<PublishedChapter> ch = pubchapters.Where(x => x.Chapter == c);
            ret.Add(new ChapterShort(ch.FirstOrDefault()?.Id ?? 0, c, chapnum?.Title ?? c.ToString(), [.. sectionInfo]));
        });
        return [.. ret];
    }
    public Book GetBook(string bibleId, bool sections, List<Published> ready, IEnumerable<PublishedChapter> publishedchapters, string book)
    {
        List<Published> myStuff = [.. ready.Where(r => r.Book == book).Distinct()];
        int? bookid = myStuff.Select(r => r.Bookid).FirstOrDefault();
        int? altbookid = myStuff.Select(r => r.Altbookid).FirstOrDefault();
        bool story = myStuff.Select(r => r.Story).FirstOrDefault();
        SectionInfo? titleInfo=null;
        SectionInfo? alttitleInfo=null;
        if (bookid != null)
        {
            Section booksection = _context.Sections.Where(s => s.Id == bookid).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
            titleInfo = GetSectionInfo(booksection, myStuff);
        }
        if (altbookid != null)
        {
            Section altbooksection = _context.Sections.Where(s => s.Id == altbookid).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
            alttitleInfo = GetSectionInfo(altbooksection, myStuff);
        }
        IEnumerable<MovementShort> movements = ReadyMovements(myStuff);
        ChapterShort [] chapters = ReadyChapters(sections, myStuff, publishedchapters, book);
        Book thisbook = new()
        {
            Id = bookid ?? 0,
            Bible_id = bibleId,
            Book_id = book,
            Name = titleInfo?.Title ?? book,
            Name_long = alttitleInfo?.Title ?? titleInfo?.Title ?? book,
            Name_alt = alttitleInfo?.Title ?? book,
            Title_audio = titleInfo?.Title_audio ?? [],
            Title_audio_alt = alttitleInfo?.Title_audio ?? [],
            Images = titleInfo?.Images ?? [],
            Movements = [.. movements],
            Chapters = chapters,
            Audio_notes = titleInfo?.Audio_notes ?? [],
            Story = story //overriden if scripture to false
        };
        return thisbook;
    }
    public List<Book> GetBibleBooks(string bibleId, bool beta, bool sections, string? book)
    {
        Bible? bible = ReadyBibles(beta, bibleId).FirstOrDefault();
        return bible != null ? GetBibleBooks(bible, beta, sections, book) : throw new Exception("Bible not found");
    }
    public List<Book> GetHelpsBooks(string bibleId, bool sections, string? book)
    {
        Bible? bible = HelpsReadyBibles(bibleId).FirstOrDefault();
        return bible != null ? GetHelpsBooks(bible, sections, book) : throw new Exception("Bible not found");
    }
    private List<PublishedChapter> PublishedChapters(string bibleId, string book)
    {
        return [.. _context.Published.Where(ch => ch.Bibleid == bibleId && ch.Book == book)
                        .Select(p => new PublishedChapter
                            {
                                Bibleid = p.Bibleid,
                                Book = p.Book,
                                Chapter = p.DestinationChapter() ?? 0
                            }).Distinct()];
        //.FromSqlRaw("SELECT * FROM get_published_chapters({0}, {1})", bibleId, book)
        //.Distinct()];
    }
    private List<Book> GetBooks(string bibleId, bool sections, List<Published> ready)
    {
        List<Book> books = [];
        //merge question
        //IEnumerable<string> publishedbooks = ready.Where(r => r.Passagetype == null).Select(p => p.Book ?? "").Distinct();
        IEnumerable<string> publishedbooks = ready.Select(p => p.Book ?? "").Distinct();

        publishedbooks.ToList().ForEach(b => {
            List<PublishedChapter> publishedchapters = PublishedChapters(bibleId, b);
            books.Add(GetBook(bibleId, sections, ready, publishedchapters, b));
        });
        books.Sort();
        return books;
    }
    private List<Book> GetBibleBooks(Bible bible, bool beta, bool sections,
                                     string? book)
    {
        return GetBooks(bible.BibleId, sections, [.. Ready(false, beta, bible?.Id, book)]);
    }
    private List<Book> GetHelpsBooks(Bible bible, bool sections, string? book)
    {
        return GetBooks(bible.BibleId, sections, [.. HelpsReady(false, bible?.Id, book)]);
    }
    private ChapterWrapper GetBookChapters(IQueryable<Published> ready, IEnumerable<PublishedChapter> publishedchapters, string bookId, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        ChapterWrapper wrapper = new(bookId);
        List<ChapterInfo> info = wrapper.Chapters;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        List<Published> vernacularq = [.. ready.Where(r => r.Passagetype == null).Include(r => r.Section).ThenInclude(s =>s.TitleMediafile)];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (justthissection != null)
            vernacularq = [.. vernacularq.Where(p => p.Sectionid == int.Parse(justthissection))];
        //if there are passages that cross chapters, we may have more than one chapter number
        ChapterShort[] chapters = ReadyChapters(true, vernacularq,publishedchapters, bookId).Where(c => justthischapter == null || c.Chapter == int.Parse(justthischapter)|| c.Id == int.Parse(justthischapter)).ToArray();

        List<Published> readyList = [.. ready];
        List<Published> chapterrows = [.. readyList.Where(r => r.Book == bookId && r.Passagetype == CHAPTER)];

        //Dictionary<Section, IOrderedEnumerable<Section>> allmovements = MovementSections(ReadyMovements(vernacular), vernacular);
        int noteid = NoteType().Id;
        foreach (ChapterShort chapter in chapters)
        {
            List<Published> chapterpsgs = vernacularq.Where(p => p.DestinationChapter() == chapter.Chapter).ToList();
            List<MovementShort> movements = ReadyMovements(chapterpsgs);
            Published? chapnum = chapterrows.Where(r => r.Reference == CHAPTER+" "+chapter.Chapter.ToString()).FirstOrDefault();
            Audio? audio = chapnum != null ? GetAudio(_mediafileService.GetLatest(chapnum.Passageid)) : null;
            Image [] graphics = chapnum != null ? PassageGraphic(chapnum) : [];
            List<SectionInfo> sectionInfo = [];
            List<AudioNote> chapternotes = [];
            info.Add(new ChapterInfo(chapter, chapnum?.Title ?? chapter.Chapter.ToString(),
            chapterpsgs.OrderBy(x => x.Sequencenum), movements, audio, graphics, GetSectionInfoList(chapter.Sections, readyList, sections, justthissection), chapternotes, readyList));
            //Debug.WriteLine($"XXX chapter {chapter.Chapter} : {stopwatch.ElapsedMilliseconds} ms");
        }
        return wrapper;
    }
    private SectionInfo [] GetSectionInfoList(SectionShort [] sections, List<Published> ready, bool showSections, string? justthissection)
    {
        if (!showSections)
            justthissection = "-1";
        int? sectionid = justthissection == null ? null : int.Parse(justthissection);
        List<SectionInfo> sectionInfo = [];
        sections.ToList().ForEach(s => {
            if (sectionid == null || s.Id == sectionid)
            {
                sectionInfo.Add(GetSectionInfo(s.GetSection(), ready));
            }
        });
        return [.. sectionInfo];
    }
    public ChapterWrapper GetBibleBookChapters(string bibleId, string bookId, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        Bible bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        IQueryable<Published> ready = Ready(false, beta, bible.Id, bookId);
        return GetBookChapters(ready, PublishedChapters(bibleId, bookId), bookId, sections, justthischapter, justthissection);
    }
    public ChapterWrapper GetHelpsBookChapters(string bibleId, string bookId, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        Bible bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        IQueryable<Published> ready = HelpsReady(false, bible.Id, bookId);
        return GetBookChapters(ready, PublishedChapters(bibleId, bookId), bookId, sections, justthischapter, justthissection);
    }
    private SectionInfo GetSectionInfo(Section s, List<Published> ready, List<AudioNote>? chapternotes = null, int chapter = 0)
    {
        NoteLevel level = NoteLevel.Section;
        List<AudioNote> sectionnotes=[];
        List<PassageInfo> passages = [];
        IOrderedEnumerable<Published> readyPassages = ready.Where(r => r.Sectionid ==s.Id).Distinct().OrderBy(x => x.Sequencenum);
        PassageInfo? curPassage = null;
        bool skipPassage = false;
        bool isPublic = readyPassages.Any(p => p.Ispublic);
        Audio? sectionTitle = GetAudio(s.TitleMediafile);
        if (sectionTitle != null)
        {
            passages.Add(new PassageInfo(new Passage(sectionTitle.Id, 0, null, null, s.Id, null, s.Name,
                0, 0, 0, 0, "SectionTitle"), OBTTypeEnum.title, sectionTitle, s.Name));
        }
        Published? first = readyPassages.FirstOrDefault();
        Image [] sectiongraphic = [];
        if (first != null)
            sectiongraphic = GraphicInfo(first.Sectionimage, first.Sectionimageid, first.Sectionimagedate);
        foreach (Published p in readyPassages)
        {
            if (p.Passagetype == null)
            {
                skipPassage = chapter > 0 && p.Startchapter != chapter && p.Endchapter != chapter;
                if (!skipPassage)
                {
                    Mediafile? media = p.Mediafile;
                    curPassage = new PassageInfo(new Passage(p),
                        p.Isscripture ? OBTTypeEnum.scripture : OBTTypeEnum.extra,
                                GetAudio(p.Mediafile),
                                media?.Transcription);
                    passages.Add(curPassage);
                }
                else
                    curPassage = null;
                level = NoteLevel.Passage;
            }
            else if (p.Passagetype == CHAPTER) //chapter
            {
                if (int.TryParse(p.Reference?.Split(" ") [1], out int chnum))
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
                AudioNote note = new (new Passage(p), OBTTypeEnum.audio_note, GetAudio(p.Mediafile), p.Transcription, p.Sharedresource, PassageGraphic(p), GetAudio(p.Sharedresource?.TitleMediafile));
                if (level == NoteLevel.Chapter && chapternotes != null)
                {
                    chapternotes.Add(note);
                }
                else if (level == NoteLevel.Section)
                {
                    sectionnotes.Add(note);
                }
                else if (level == NoteLevel.Passage && curPassage != null) //passage note
                {
                    curPassage.Audio_notes.Add(note);
                }

            }
        }
        return new SectionInfo(s, sectionTitle, sectiongraphic, [.. passages], [.. sectionnotes], isPublic);
    }
    private MovementWrapper GetBookMovements(string bibleId, List<Published> ready, List<PublishedChapter> publishedChapters, string bookId, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetBibleBookMovements");
        MovementWrapper movementWrapper = new (bookId);
        //WriteLog("ready");
        Book book = GetBook(bibleId, showSections, ready, publishedChapters, bookId);
        //WriteLog("getbook");
        if (book == null)
            return movementWrapper;
        //Debug.WriteLine("{0} {1}",DateTime.Now.ToLongTimeString(), DateTime.Now.Ticks-ticks);
        movementWrapper.Name = book.Name ?? "";
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id))
            movementId = id;
        List<MovementShort> allmovements = ReadyMovements(ready);
        List<MovementShort> movements = movementId != null ? allmovements.Where(m => m.Id == movementId).ToList() : allmovements;

        List<MovementInfo> info = movementWrapper.Movements;

        movements.ForEach(m => {
            Section section = _context.Sections.Where(s => s.Id == m.Id).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
            SectionInfo movementInfo = GetSectionInfo(section, ready);
            info.Add(new MovementInfo(m.Id, m.Title, movementInfo.Title_audio.ElementAtOrDefault(0), movementInfo.Images, m.Sections, GetSectionInfoList(m.Sections, ready, showSections, justthissection), ready, Array.IndexOf([.. allmovements], m) + 1, movementInfo.Audio_notes));

        });
        //WriteLog("done");

        return movementWrapper;
    }
    //return all the sections for the bible/book in one list
    public List<MovementWrapper> GetBibleBookAll(string bibleId, string? bookId, bool beta)
    {
        List<MovementWrapper> all = [];
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        List<Book> books = GetBibleBooks(bible, beta,true, bookId);
        books.ForEach(b => {
            if (b.Book_id != null)
            {
                MovementWrapper movementWrapper = GetBibleBookMovements(bibleId, b.Book_id, beta, true);
                all.Add(movementWrapper);
            }
        });
        return all;
    }

    public MovementWrapper GetBibleBookMovements(string bibleId, string bookId, bool beta, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetBibleBookMovements");
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));
        List<Published> ready = [.. Ready(false, beta, bible.Id, bookId).Include(r => r.Titlemediafile)];
        MovementWrapper m = GetBookMovements(bibleId, ready, PublishedChapters(bibleId, bookId), bookId, showSections, justthismovement, justthissection);
        return m;
    }
    public MovementWrapper GetHelpsBookMovements(string bibleId, string bookId, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetHelpsBookMovements");
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));
        List<Published> ready = [.. HelpsReady( false, bible.Id, bookId).Include(r => r.Titlemediafile)];
        return GetBookMovements(bibleId, ready, PublishedChapters(bibleId, bookId), bookId, showSections, justthismovement, justthissection);
    }

}
