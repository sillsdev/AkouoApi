using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using AkouoApi.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkouoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    #region DBSet
    public DbSet<Artifactcategory> Artifactcategorys => Set<Artifactcategory>();
    public DbSet<Bible> Bibles => Set<Bible>();
    public DbSet<Graphic> Graphics => Set<Graphic>();
    public DbSet<Mediafile> Mediafiles => Set<Mediafile>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Passagetype> Passagetypes => Set<Passagetype>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<PublishedBible> Vwpublishedbibles => Set<PublishedBible>();
    public DbSet<PublishedGeneral> Vwpublishedgeneral => Set<PublishedGeneral>();
    public DbSet<PublishedScripture> Vwpublishedscripture => Set<PublishedScripture>();

    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Sharedresource> Sharedresources => Set<Sharedresource>();
    #endregion
    public static void LowerCaseDB(ModelBuilder builder)
    {
        foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
        {
#pragma warning disable EF1001 // Internal EF Core API usage.
            if (entity is EntityType { IsImplicitlyCreatedJoinEntityType: true })
#pragma warning restore EF1001 // Internal EF Core API usage.
            {
                continue;
            }
            // Replace table names
            //05/16/22 This doesn't work anymore so use the [Table] schema notation--and now it does...
            entity.SetTableName(entity.GetTableName()?.ToLower());

            // Replace column names
            foreach (IMutableProperty property in entity.GetProperties())
            {
                property.SetColumnName(property.Name.ToLower());
            }

            foreach (IMutableKey key in entity.GetKeys())
            {
                key.SetName(key.GetName() ?? "".ToLower());
            }

            foreach (IMutableForeignKey key in entity.GetForeignKeys())
            {
                key.SetConstraintName(key.GetConstraintName() ?? "".ToLower());
            }

            foreach (IMutableIndex index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToLower());
            }
        }
    }
    private static void DefineRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artifactcategory>().HasKey(a => a.Id);
        modelBuilder.Entity<Artifactcategory>().HasOne(p => p.TitleMediafile)
            .WithMany().HasForeignKey(p => p.TitleMediafileId);
        EntityTypeBuilder<Bible> bib = modelBuilder
            .Entity<Bible>();
        bib.HasOne(o => o.IsoMediafile)
             .WithMany().HasForeignKey(x => x.IsoMediafileId);
        bib.HasOne(o => o.BibleMediafile)
            .WithMany().HasForeignKey(x => x.BibleMediafileId);

        EntityTypeBuilder<Mediafile> mf = modelBuilder
            .Entity<Mediafile>();
        mf.HasKey(m => m.Id);
        mf.HasOne(o => o.Passage)
            .WithMany().HasForeignKey(o => o.PassageId);
        mf.HasOne(o => o.ResourcePassage)
            .WithMany().HasForeignKey(o => o.ResourcePassageId);
        
        EntityTypeBuilder<Organization> orgEntity = modelBuilder.Entity<Organization>();
        orgEntity.HasKey(o => o.Id);

        EntityTypeBuilder<Passagetype> passType = modelBuilder.Entity<Passagetype>();
        passType.HasKey(p => p.Id);

        EntityTypeBuilder<Plan> plan = modelBuilder.Entity<Plan>();
        plan.HasKey(p => p.Id);
        plan.HasOne(p => p.Project)
            .WithMany().HasForeignKey(p => p.ProjectId);

        EntityTypeBuilder<Project> proj = modelBuilder.Entity<Project>();
        proj.HasKey(p => p.Id);
        proj.HasOne(p => p.Organization)
            .WithMany().HasForeignKey(p => p.OrganizationId);

        EntityTypeBuilder<Sharedresource> sr = modelBuilder.Entity<Sharedresource>();
        sr.HasKey(s => s.Id);
        sr.HasOne(p => p.TitleMediafile)
            .WithMany().HasForeignKey(p => p.TitleMediafileId);
        sr.HasOne(p => p.Passage)
            .WithMany().HasForeignKey(p => p.PassageId);
        sr.HasOne(p => p.ArtifactCategory)
            .WithMany().HasForeignKey(p => p.ArtifactCategoryId);
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        _ = builder.HasPostgresExtension("uuid-ossp");
        //make all query items lowercase to send to postgres...
        LowerCaseDB(builder);

        DefineRelationships(builder);
    }

}



