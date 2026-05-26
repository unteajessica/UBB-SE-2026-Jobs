using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PussyCats.App.Configuration;
using PussyCats.App.RepositoryProxies;
using PussyCats.Library.ServiceProxies;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Services.Auth;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.CompatibilityService;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.CooldownService;
using PussyCats.Library.Services.Developers;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.ImageStorage;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Library.Services.Preferences;
using PussyCats.Library.Services.Recommendations;
using PussyCats.Library.Services.SkillGapService;
using PussyCats.Library.Services.Skills;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.UserRecommendationService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.UserSkillService;
using PussyCats.Library.Services.UserStatusService;

namespace PussyCats_App;

public partial class App : Application
{
    private Window? _window;
    private IServiceProvider serviceProvider = null!;

    public static Window? MainAppWindow { get; private set; }
    public static IServiceProvider Services => ((App)Current).serviceProvider;

    public App()
    {
        InitializeComponent();
        UIDispatcher.Queue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainAppWindow = _window;
        _window.Activate();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var apiConfiguration = ApiConfigurationLoader.Load();
        services.AddSingleton(apiConfiguration);
        services.AddSingleton<SessionContext>();
        services.AddTransient<JwtForwardingHandler>();

        RegisterServiceProxy<IAuthService, AuthServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IChatService, ChatServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompatibilityService, CompatibilityServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompletenessService, CompletenessServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompanyRecommendationService, CompanyRecommendationServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompanyService, CompanyServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompanyStatusService, CompanyStatusServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICooldownService, CooldownServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IDeveloperService, DeveloperServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IDocumentService, DocumentServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IImageStorageService, ImageStorageServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IJobService, JobServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IJobSkillService, JobSkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ILocalFileStorageService, FileStorageServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IMatchService, MatchServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IPersonalityTestService, PersonalityTestServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IPreferenceService, PreferenceServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IRecommendationService, RecommendationServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ISkillGapService, SkillGapServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ISkillService, SkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ISkillTestService, SkillTestServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserProfileService, UserProfileServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserRecommendationService, UserRecommendationServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserService, UserServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserSkillService, UserSkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserStatusService, UserStatusServiceProxy>(services, apiConfiguration);

        services.AddHttpClient<IDocumentRepository, DocumentRepositoryProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl))
            .AddHttpMessageHandler<JwtForwardingHandler>();
        services.AddTransient<ILocalDocumentFileService>(provider => new DocumentService(
            provider.GetRequiredService<IDocumentRepository>(),
            provider.GetRequiredService<ILocalFileStorageService>(),
            provider.GetRequiredService<IUserService>(),
            new CvParsingService()));

        services.AddHttpClient<CvExportProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl))
            .AddHttpMessageHandler<JwtForwardingHandler>();

        RegisterViewModels(services);
    }

    private static void RegisterServiceProxy<TService, TProxy>(
        IServiceCollection services,
        ApiConfiguration apiConfiguration)
        where TService : class
        where TProxy : class, TService
    {
        services.AddHttpClient<TService, TProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl))
            .AddHttpMessageHandler<JwtForwardingHandler>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        var viewModelTypes = typeof(App).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("ViewModel", StringComparison.Ordinal));

        foreach (var viewModelType in viewModelTypes)
        {
            services.AddTransient(viewModelType);
        }
    }
}
