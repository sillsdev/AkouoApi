﻿using AkouoApi.Data;
using AkouoApi.Models;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using System.Diagnostics;



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
    private ChapterShort [] ReadyChapters(string bibleId, IEnumerable<Published> publishedpassages, string bookId)
    {
        List<ChapterShort> ret = new ();
        IEnumerable<int> chapternums = publishedpassages.Select(p => p.DestinationChapter()??0);
        List<int> chapters = chapternums.Where(c => c != 0).ToImmutableSortedSet().ToList();
        chapters.ForEach(c => {
            Published? chapnum = publishedpassages.Where(r => r.Passagetype == CHAPTER && r.Reference == CHAPTER+" "+c.ToString()).FirstOrDefault();
            List<Published> chapterpsgs = publishedpassages.Where(p => p.DestinationChapter() == c).ToList();
            List<Section> readySections = chapterpsgs.Select(p => p.Section).Select(s => s!).Distinct(new RecordEqualityComparer<Section>()).OrderBy(x => x.Sequencenum).ToList();
            List<SectionShort> sectionInfo = new ();
            List<AudioNote> chapternotes = new();
            foreach (Section s in readySections)
            {
                sectionInfo.Add(new SectionShort(s));
            }
            IQueryable<PublishedChapter> ch = _context.Vwpublishedchapters.Where(ch => ch.Bibleid == bibleId && ch.Book == bookId && ch.Chapter == c);
            ret.Add(new ChapterShort(ch.FirstOrDefault()?.Id??0, c, chapnum?.Title ?? c.ToString(), sectionInfo.ToArray()));
        });
        return ret.ToArray();
    }
    public Book GetBook(string bibleId, List<Published> ready, string book)
    {
        List<Published> myStuff = ready.Where(r => r.Book == book).ToList();
        int? bookid = myStuff.Select(r => r.Bookid).FirstOrDefault();
        int? altbookid = myStuff.Select(r => r.Altbookid).FirstOrDefault();
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
        ChapterShort [] chapters = ReadyChapters(bibleId, myStuff, book);
        return new Book()
        {
            Id = bookid??0,
            Book_id = book,
            Name = titleInfo?.Title ?? book,
            Name_long = alttitleInfo?.Title ?? titleInfo?.Title ?? book,
            Name_alt = alttitleInfo?.Title ?? book,
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
    public List<Book> GetHelpsBooks(string bibleId, bool scripture, string? book)
    {
        Bible? bible = HelpsReadyBibles(bibleId).FirstOrDefault();
        return bible != null ? GetHelpsBooks(bible, scripture, book) : throw new Exception("Bible not found");
    }
    private List<Book> GetBooks(string bibleId, List<Published> ready)
    {
        List<Book> books = new();
        //merge question
        //IEnumerable<string> publishedbooks = ready.Where(r => r.Passagetype == null).Select(p => p.Book ?? "").Distinct();
        IEnumerable<string> publishedbooks = ready.Select(p => p.Book ?? "").Distinct();
        publishedbooks.ToList().ForEach(b => {
            books.Add(GetBook(bibleId, ready, b));
        });
        books.Sort();
        return books;
    }
    private List<Book> GetBibleBooks(Bible bible, bool scripture, bool beta, 
                                     string? book)
    {
        return GetBooks(bible.BibleId, Ready(scripture, false, beta, bible?.Id, book).ToList());
    }
    private List<Book> GetHelpsBooks(Bible bible, bool scripture, string? book)
    {
        return GetBooks(bible.BibleId, HelpsReady(scripture, false, bible?.Id, book).ToList());
    }
    private ChapterWrapper GetBookChapters(string bibleId, IQueryable<Published> ready, string bookId, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        ChapterWrapper wrapper = new(bookId);
        List<ChapterInfo> info = wrapper.Chapters;
        IQueryable<Published> vernacularq = ready.Where(r => r.Passagetype == null).Include(r => r.Section).ThenInclude(s =>s.TitleMediafile);
         if (justthissection != null)
            vernacularq = vernacularq.Where(p => p.Sectionid == int.Parse(justthissection));
        //if there are passages that cross chapters, we may have more than one chapter number
        ChapterShort[] chapters = ReadyChapters(bibleId, vernacularq,bookId).Where(c => justthischapter == null || c.Chapter == int.Parse(justthischapter)|| c.Id == int.Parse(justthischapter)).ToArray();

        //Dictionary<Section, IOrderedEnumerable<Section>> allmovements = MovementSections(ReadyMovements(vernacular), vernacular);
        int noteid = NoteType().Id;
        int chapterid = ChapterType().Id;
        foreach (ChapterShort chapter in chapters)
        {
            List<Published> chapterpsgs = vernacularq.ToList().Where(p => p.DestinationChapter() == chapter.Chapter).ToList();
            List<MovementShort> movements = ReadyMovements(chapterpsgs);
            Published? chapnum = vernacularq.Where(r => r.Book == bookId && r.Passagetype == CHAPTER && r.Reference == CHAPTER+" "+chapter.Chapter.ToString()).FirstOrDefault();
            Audio? audio = chapnum != null ? GetAudio(_mediafileService.GetLatest(chapnum.Passageid)) : null;
            Image [] graphics = chapnum != null ?GetGraphicImages(chapnum.Passageid, "passage") : Array.Empty<Image>();
            List<SectionInfo> sectionInfo = new ();
            List<AudioNote> chapternotes = new();
            info.Add(new ChapterInfo(chapter, chapnum?.Title ?? chapter.Chapter.ToString(),
            chapterpsgs.OrderBy(x => x.Sequencenum), movements, audio, graphics, GetSectionInfoList(chapter.Sections, ready.ToList(), sections, justthissection), chapternotes, ready));
        };    
        return wrapper;
    }
    private SectionInfo[] GetSectionInfoList(SectionShort[] sections, List<Published> ready, bool showSections, string? justthissection)
    {
        if (!showSections)
            justthissection = "-1";
        int? sectionid = justthissection == null ? null : int.Parse(justthissection);
        List<SectionInfo> sectionInfo = new ();
        sections.ToList().ForEach(s => {
            if (sectionid == null || s.Id == sectionid)
            {
                sectionInfo.Add(GetSectionInfo(s.GetSection(), ready));
            }
        });
        return sectionInfo.ToArray();
    }
    public ChapterWrapper GetBibleBookChapters(string bibleId, string bookId, bool scripture, bool beta, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        Bible bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        IQueryable<Published> ready = Ready(scripture,false, beta, bible.Id, bookId);
        return GetBookChapters(bibleId, ready, bookId, sections, justthischapter, justthissection);
    }
    public ChapterWrapper GetHelpsBookChapters(string bibleId, string bookId, bool scripture, bool sections, string? justthischapter = null, string? justthissection = null)
    {
        Bible bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw new Exception("Bible not found");
        IQueryable<Published> ready = HelpsReady(scripture,false, bible.Id, bookId);
        return GetBookChapters(bibleId, ready, bookId, sections, justthischapter, justthissection);
    }
    private SectionInfo GetSectionInfo(Section s, List<Published> ready, List<AudioNote>? chapternotes=null,  int chapter=0) 
    {
        NoteLevel level = NoteLevel.Section;
        List<AudioNote> sectionnotes=new();
        List<PassageInfo> passages = new ();
        IOrderedEnumerable<Published> readyPassages = ready.Where(r => r.Sectionid ==s.Id).OrderBy(x => x.Sequencenum);
        PassageInfo? curPassage = null;
        bool skipPassage = false;
        bool isPublic = readyPassages.Any(p => p.IsPublic);
        Audio? sectionTitle = GetAudio(s.TitleMediafile);
        if (sectionTitle != null)
        {
            passages.Add(new PassageInfo(new Passage(sectionTitle.Id, 0, null, null, s.Id, null, s.Name, 
                0, 0, 0, 0, "SectionTitle"), OBTTypeEnum.title, sectionTitle, s.Name));
        }
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
        return new SectionInfo(s, sectionTitle, GetGraphicImages(s.Id, "section"), passages.ToArray(), sectionnotes.ToArray(), isPublic);
    }
    private MovementWrapper GetBookMovements(string bibleId, List<Published> ready, string bookId, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetBibleBookMovements");
        MovementWrapper movementWrapper = new (bookId);
        //WriteLog("ready");
        Book book = GetBook(bibleId, ready, bookId);
        //WriteLog("getbook");
        if (book == null)
            return movementWrapper;
        //Debug.WriteLine("{0} {1}",DateTime.Now.ToLongTimeString(), DateTime.Now.Ticks-ticks);
        movementWrapper.Name = book.Name??"";
        int? movementId = null;
        if (int.TryParse(justthismovement, out int id))
            movementId = id;
        List<MovementShort> allmovements = ReadyMovements(ready);
        List<MovementShort> movements = movementId != null ? allmovements.Where(m => m.Id == movementId).ToList() : allmovements;
        
        List<MovementInfo> info = movementWrapper.Movements;

        movements.ForEach(m => {
            Section section = _context.Sections.Where(s => s.Id == m.Id).Include(s => s.TitleMediafile).FirstOrDefault() ?? new Section();
            SectionInfo movementInfo = GetSectionInfo(section, ready);
            info.Add(new MovementInfo(m.Id, m.Title, movementInfo.Title_audio.ElementAtOrDefault(0), movementInfo.Images, m.Sections, GetSectionInfoList(m.Sections, ready, showSections, justthissection), ready, Array.IndexOf(allmovements.ToArray(), m) + 1,movementInfo.Audio_notes));
        });

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

    public MovementWrapper GetBibleBookMovements(string bibleId, string bookId, bool scripture, bool beta, bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetBibleBookMovements");
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));
        List<Published> ready = Ready(scripture,false, beta, bible.Id, bookId).Include(r => r.Titlemediafile).ToList();
        return GetBookMovements(bibleId, ready, bookId, showSections, justthismovement, justthissection);
    }
    public MovementWrapper GetHelpsBookMovements(string bibleId, string bookId, bool scripture,bool showSections, string? justthismovement = null, string? justthissection = null)
    {
        //WriteLog("GetHelpsBookMovements");
        Bible? bible = _context.Bibles.Where(b => b.BibleId == bibleId).FirstOrDefault() ?? throw (new Exception("Bible not found"));
        List<Published> ready = HelpsReady(scripture,false, bible.Id, bookId).Include(r => r.Titlemediafile).ToList();
        return GetBookMovements(bibleId, ready, bookId, showSections, justthismovement, justthissection);
    }

}
