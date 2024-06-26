﻿using Microsoft.EntityFrameworkCore;
using AkouoApi.Data;
using AkouoApi.Models;
using System.Net;
using System.Collections.Concurrent;

namespace AkouoApi.Services
{
    public class MediafileService// : BaseArchiveService<Mediafile>
    {
        private readonly AppDbContext _context;
        private readonly IS3Service _s3Service;
        private readonly ConcurrentDictionary<int, string> _cache;


        public MediafileService(
                           AppDbContext context,
                           IS3Service s3Service)
        {
            _context = context;
            _s3Service = s3Service;
            _cache = new ConcurrentDictionary<int, string>();
        }
        /*

        public Mediafile? GetFromFile(int plan, string s3File)
        {
            IEnumerable<Mediafile> files = MyRepository.Get(); //bypass user check
            return files.SingleOrDefault(p => p.S3File == s3File && p.PlanId == plan);
        }
        */
        private string DirectoryName(Plan? plan)
        {
            if (plan == null)
                return "";
            Project? proj = plan.Project;
            proj ??= _context.Projects
                    .Where(p => p.Id == plan.ProjectId)
                    .Include(p => p.Organization)
                    .FirstOrDefault();
            Organization? org = proj?.Organization;
            if (org == null && proj?.OrganizationId != null)
                org = _context.Organizations
                    .Where(o => o.Id == proj.OrganizationId)
                    .FirstOrDefault();
            return org != null ? org.Slug + "/" + plan.Slug : throw new Exception("No org in DirectoryName");
        }

        public string DirectoryName(int planId)
        {
            Plan? plan = _context.Plans
                .Include(p => p.Project)
                .ThenInclude(p => p.Organization)
                .Where(p => p.Id == planId)
                .FirstOrDefault();
            return plan != null ? DirectoryName(plan) : "";
        }
        public string DirectoryName(Mediafile entity)
        {
            int id = entity.Plan?.Id ?? entity.PlanId;
            return _cache.GetOrAdd(id, value => DirectoryName(id));
        }

        public async Task<string> GetNewFileNameAsync(Mediafile mf, string suffix = "")
        {
            string ext = Path.GetExtension(mf.OriginalFile)??"";
            string newfilename = Path.GetFileNameWithoutExtension(mf.OriginalFile ?? "") +suffix + ext;
            return mf.SourceMedia == null && await _s3Service.FileExistsAsync(newfilename, DirectoryName(mf))
                ? Path.GetFileNameWithoutExtension(mf.OriginalFile)
                    + "__"
                    + Guid.NewGuid()
                    + suffix
                    + ext
                : newfilename;
        }
        private Mediafile? Get(int id)
        {
            return _context.Mediafiles.Where(m => m.Id == id)
                .FirstOrDefault();
        }
        public string? GetFileSignedUrl(int id)
        {
            Mediafile? mf = Get(id);
            return mf == null
                ? null
                : _s3Service
                    .SignedUrlForGet(mf.S3File ?? "", DirectoryName(mf), mf.ContentType ?? "")
                    .Message;
        }

        public async Task<S3Response> GetFile(int id)
        {
            Mediafile? mf = Get(id);
            if (mf == null || mf.S3File == null)
            {
                return new S3Response { Message = "", Status = HttpStatusCode.NotFound };
            }
            Plan? plan = _context.Plans.Where(p => p.Id == mf.PlanId).Include(p => p.Project).FirstOrDefault();
            if (
                mf.S3File.Length == 0
                || !(await _s3Service.FileExistsAsync(mf.S3File, DirectoryName(plan)))
            )
                return new S3Response
                {
                    Message = mf.S3File.Length > 0 ? mf.S3File : "",
                    Status = HttpStatusCode.NotFound
                };

            S3Response response = await _s3Service.ReadObjectDataAsync(
                mf.S3File ?? "",
                DirectoryName(plan)
            );
            response.Message = mf.OriginalFile ?? "";
            return response;
        }
        public Mediafile? GetLatest(int passageId)
        {
            return _context.Mediafiles
                .Where(mf => mf.PassageId == passageId && mf.ReadyToShare)
                .OrderByDescending(mf => mf.VersionNumber)
                .FirstOrDefault();
        }
        public Mediafile? GetLatestNote(Passage passage)
        {
            if (passage.SharedResourceId != null)
            {
                Sharedresource? sr = _context.Sharedresources
                    .Where(sr => sr.Id == passage.SharedResourceId)
                    .FirstOrDefault();
                if (sr?.PassageId != null)
                {
                    return GetLatest((int)sr.PassageId);
                }
            }
            return GetLatest(passage.Id);
        }
        /*
        public List<Mediafile> GetIPMedia(IQueryable<Organization> orgs)
        {
            IEnumerable<Intellectualproperty>? ip = _context.IntellectualPropertys.Join(orgs, ip => ip.OrganizationId, o => o.Id, (ip, o) => ip).ToList();
            return ip.Join(_context.Mediafiles, ip => ip.ReleaseMediafileId, m => m.Id, (ip, m) => m).ToList();
        }
        */

    }
}
