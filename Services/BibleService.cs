﻿using AkouoApi.Data;
using AkouoApi.Models;
using System.Diagnostics;

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

    public List<BibleShort> GetBibles(bool publishBeta)
    {
        return ShortBibles(ReadyBibles(publishBeta).ToList());
    }
    public List<BibleFull> GetBible(string bibleId, bool publishBeta)
    {
        return FullBibles(ReadyBibles(publishBeta, bibleId).ToList());
    }
    public List<BibleShort> GetBibleByIso(string iso, bool publishBeta)
    {
        return ShortBibles(ReadyBibles(publishBeta).ToList().Where(o => o.Iso == iso).ToList());
    }
    public List<OBTType> GetBibleOBTTypes(string bibleId, bool beta)
    {
        return GetBibleOBTTypes(_context.Bibles.Where(o => o.BibleId == bibleId).FirstOrDefault(), beta);
    }
    public List<OBTType> GetBibleOBTTypes(Bible? bible, bool publishBeta)
    {
        List<OBTType> obts = new();
        if (bible != null)
        {
            List<Published> all = Ready(true, false, publishBeta, bible.Id).ToList();
            if (all.Any(all => all.Passagetype == null))
            {
                obts.Add(new OBTType(OBTTypeEnum.scripture));
                if (all.Any(all => (all.Book ?? "") != ""))
                {
                    obts.Add(new OBTType(OBTTypeEnum.book));
                }
            }
            int chapterid = ChapterType().Id;
            if (AnyMovements(all))
            {
                obts.Add(new OBTType(OBTTypeEnum.movement));
            }
            if (all.Any(all => all.Passagetype == CHAPTER))
            {
                obts.Add(new OBTType(OBTTypeEnum.chapter));
            }
            int noteid = NoteType().Id;

            IEnumerable<Published> notes = all.Where(all => all.Passagetype == NOTE);
            int introcount = notes.Where(n => n.Level < SectionLevel.Section).Count();
            Published [] ordered = all.Where(n => n.Level == SectionLevel.Section).OrderBy(a => a.Sectionsequence).ThenBy(a => a.Sequencenum).ToArray();
            IEnumerable<Published> maybechapter = notes.Where(n => n.Level == SectionLevel.Section);
            foreach (Published note in maybechapter)
            {
                int ix = Array.IndexOf(ordered, note);
                while (ix > 0 && ordered [ix].Passagetype == NOTE)
                {
                    ix--;
                }
                if (ordered [ix].Passagetype == CHAPTER)
                {
                    introcount++;
                }
            }
            if (introcount > 0)
            {
                obts.Add(new OBTType(OBTTypeEnum.introduction));
            }
            int appendix = 0; //TODO EH - check last section?
            if (appendix > 0)
            {
                obts.Add(new OBTType(OBTTypeEnum.appendix));
            }
            if (notes.Count() > introcount + appendix)
            {
                obts.Add(new OBTType(OBTTypeEnum.audio_note));
            }
            List<Published> extra = Ready(false, false, publishBeta, bible.Id).ToList();
            if (extra.Any())
            {
                obts.Add(new OBTType(OBTTypeEnum.extra));
            }
            obts.Add(new OBTType(OBTTypeEnum.title));
        }
        else
            throw (new Exception("Bible not found"));

        obts.Sort();
        return obts;
    }

    public List<NoteCategoryInfo> GetBibleNoteCategories(string bibleId, bool beta)
    {
        return GetBibleNoteCategories(_context.Vwpublishedbibles.Where(o => o.BibleId == bibleId).FirstOrDefault(), beta);
    }
    public List<NoteCategoryInfo> GetBibleNoteCategories(PublishedBible? bible, bool publishBeta)
    {
        List<NoteCategoryInfo> cats = new();
        if (bible != null)
        {
            IEnumerable<Published> r = Ready(true,false, publishBeta, bible.Id).ToList().Where(p => p.Passagetype == NOTE).ToList();
            IEnumerable<Artifactcategory> acs = r.Select(p => p.Sharedresource?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
            foreach (Artifactcategory ac in acs)
            {
                if (ac != null)
                cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
            }
            //any just used in general projects?
            r = Ready(false,false, publishBeta, bible.Id).ToList().Where(p => p.Passagetype == NOTE).ToList();
            acs = r.Select(p => p.Sharedresource?.ArtifactCategory).Select(a => a!).Distinct(new RecordEqualityComparer<Artifactcategory>());
            foreach (Artifactcategory ac in acs)
            {
                if (ac != null && !cats.Any(c => c.Id==ac.Id))
                    cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
            }
            //get the special ones
            _context.Artifactcategorys.Where(a => a.OrganizationId == bible.Organizationid && (a.Specialuse??"") != "").ToList().ForEach(ac => {
                cats.Add(new NoteCategoryInfo(ac, GetAudio(ac.TitleMediafile), GetGraphicImages(ac.Id, "category")));
            });
        } else throw (new Exception("Bible not found"));
        cats.Sort();
        return cats;
    }

}
