using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Infrastructure.Authentication;
using CareerPilot.Infrastructure.Configuration;
using CareerPilot.Infrastructure.Persistence;
using CareerPilot.Infrastructure.Persistence.Interceptors;
using CareerPilot.Infrastructure.Persistence.Repositories;
using CareerPilot.Infrastructure.Persistence.Seeding;
using CareerPilot.Infrastructure.Storage;
using CareerPilot.Infrastructure.Resumes;
using CareerPilot.Infrastructure.Resumes.Export;
using CareerPilot.Infrastructure.Resumes.Parsing;
using CareerPilot.Infrastructure.Resumes.Templates;
using CareerPilot.Infrastructure.Resumes.Rendering;
using CareerPilot.Infrastructure.Jobs.Providers;
using CareerPilot.Infrastructure.Jobs.Services;
using CareerPilot.Application.Abstractions.Resumes;
using CareerPilot.Application.Abstractions.Jobs;
using CareerPilot.Application.Resumes;
using CareerPilot.Application.Resumes.Services;
using CareerPilot.Application.Abstractions.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CareerPilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddOptions(configuration);
        services.AddPersistenceInfrastructure(configuration);
        services.AddAuthenticationInfrastructure(configuration, environment);
        services.AddStorageInfrastructure();
        services.AddResumeInfrastructure();
        services.AddJobInfrastructure();
        services.AddCommunicationInfrastructure();
        services.AddSystemManagementInfrastructure();

        return services;
    }

    private static void AddSystemManagementInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<CareerPilot.Application.Abstractions.System.ISystemHealthService, CareerPilot.Infrastructure.System.SystemHealthService>();
        services.AddScoped<CareerPilot.Application.Abstractions.System.IBackupRestoreService, CareerPilot.Infrastructure.System.BackupRestoreService>();
        services.AddSingleton<CareerPilot.Application.Abstractions.System.IOllamaModelManagerService, CareerPilot.Infrastructure.System.OllamaModelManagerService>();
        services.AddSingleton<CareerPilot.Application.Abstractions.System.IUpdateCheckerService, CareerPilot.Infrastructure.System.UpdateCheckerService>();
        services.AddSingleton<CareerPilot.Application.Abstractions.System.IDiagnosticLogService, CareerPilot.Infrastructure.System.DiagnosticLogService>();
    }

    private static void AddCommunicationInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<CareerPilot.Application.Abstractions.Communication.IEmailProvider, CareerPilot.Infrastructure.Communication.Providers.Microsoft365EmailProvider>();
        services.AddSingleton<CareerPilot.Application.Abstractions.Communication.IEmailProvider, CareerPilot.Infrastructure.Communication.Providers.GoogleEmailProvider>();
        services.AddSingleton<CareerPilot.Application.Abstractions.Communication.IEmailClassificationService, CareerPilot.Infrastructure.Communication.Services.EmailClassificationService>();
        services.AddSingleton<CareerPilot.Application.Abstractions.Communication.IReplyGenerationService, CareerPilot.Infrastructure.Communication.Services.ReplyGenerationService>();
    }

    private static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));
        services.Configure<PlaywrightOptions>(configuration.GetSection(PlaywrightOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<ResumeOptions>(configuration.GetSection(ResumeOptions.SectionName));
    }

    private static void AddStorageInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
    }

    private static void AddResumeInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IResumeTemplateCatalog, ResumeTemplateCatalog>();
        services.AddSingleton<IResumeTemplateProvider, ResumeTemplateService>();
        services.AddSingleton<IResumeRenderer, ResumeHtmlRenderer>();

        services.AddSingleton<IResumeParser, PdfResumeParser>();
        services.AddSingleton<IResumeParser, DocxResumeParser>();
        services.AddSingleton<IResumeParser, JsonResumeParser>();
        services.AddSingleton<IResumeParserRegistry, ResumeParserRegistry>();

        services.AddSingleton<IResumeExporter, PdfResumeExporter>();
        services.AddSingleton<IResumeExporter, DocxResumeExporter>();
        services.AddSingleton<IResumeExporter, JsonResumeExporter>();
        services.AddSingleton<IResumeExporterRegistry, ResumeExporterRegistry>();

        services.AddSingleton<ResumeDocumentService>();
        services.AddSingleton<TemplateService>();
        services.AddSingleton<ResumeTemplateService>();
        services.AddScoped<ResumeImportService>();
        services.AddScoped<ResumeExportService>();
    }

    private static void AddJobInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IJobProvider, GreenhouseJobProvider>();
        services.AddSingleton<IJobProvider, LeverJobProvider>();
        services.AddSingleton<IJobProvider, AshbyJobProvider>();
        services.AddSingleton<IJobProvider, WorkdayJobProvider>();
        services.AddSingleton<IJobProvider, SmartRecruitersJobProvider>();
        services.AddSingleton<IJobProvider, CompanyCareerPageJobProvider>();
        services.AddSingleton<IJobProviderRegistry, JobProviderRegistry>();

        services.AddScoped<IJobNormalizationService, JobNormalizationService>();
        services.AddScoped<IJobSynchronizationService, JobSynchronizationService>();
    }

    private static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        var connectionString = !string.IsNullOrWhiteSpace(dbOptions.ConnectionString)
            ? dbOptions.ConnectionString
            : configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContextPool<ApplicationDbContext>((serviceProvider, options) =>
        {
            var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.CommandTimeout(dbOptions.CommandTimeoutSeconds);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: dbOptions.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(interceptor);

            if (dbOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IJobRepository, JobRepository>();

        services.AddScoped<IdentitySeeder>();
    }
}
