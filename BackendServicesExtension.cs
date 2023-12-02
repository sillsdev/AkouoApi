using Amazon.S3;
using AkouoApi.Data;
using Microsoft.EntityFrameworkCore;
using static AkouoApi.Utility.EnvironmentHelpers;

namespace AkouoApi.Services;

    public static class BackendServiceExtension
    {
        public static object AddApiServices(this IServiceCollection services)
        {
            // Add services to the container.
            services.AddHttpContextAccessor();
            //services.AddScoped<AppDbContextResolver>();

            // Add the Entity Framework Core DbContext like you normally would.
            services.AddDbContext<AppDbContext>(options => {
                options.UseNpgsql(GetConnectionString());
            });
        
            services.RegisterServices();

            return services;
        }

        public static void RegisterServices(this IServiceCollection services)
        {
        services.AddScoped<BibleService>();
        services.AddScoped<LanguageService>();
        services.AddScoped<MediafileService>();
        /*
            services.AddScoped<ArtifactCategoryService>();
            services.AddScoped<GraphicService>();
            services.AddScoped<IntellectualPropertyService>();
            services.AddScoped<OrganizationService>();
            services.AddScoped<PassageService>();
            services.AddScoped<PlanService>();
            services.AddScoped<ProjectService>();
            services.AddScoped<SectionService>();
            services.AddScoped<SharedResourceService>();
            services.AddScoped<SharedResourceReferenceService>();
            services.AddScoped<UserService>();
        */
        services.AddSingleton<IS3Service, S3Service>();
        services.AddAWSService<IAmazonS3>();

    }

        private static string GetConnectionString()
        {
            return GetVarOrDefault("SIL_TR_CONNECTIONSTRING", "");
        }
    }