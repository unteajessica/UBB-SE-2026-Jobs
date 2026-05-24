using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PussyCats.App.Configuration;
using PussyCats.App.RepositoryProxies;
using PussyCats.Library.ServiceProxies;
using PussyCats.Library.Repositories.Chats;
using PussyCats.Library.Repositories.Companies;
using PussyCats.Library.Repositories.Documents;
using PussyCats.Library.Repositories.Jobs;
using PussyCats.Library.Repositories.Matches;
using PussyCats.Library.Repositories.Messages;
using PussyCats.Library.Repositories.PersonalityTests;
using PussyCats.Library.Repositories.Recommendations;
using PussyCats.Library.Repositories.Skills;
using PussyCats.Library.Repositories.SkillTests;
using PussyCats.Library.Repositories.Users;
using PussyCats.Library.Services.ChatService;
using PussyCats.Library.Services.CompanyService;
using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.Documents;
using PussyCats.Library.Services.FileStorage;
using PussyCats.Library.Services.Jobs;
using PussyCats.Library.Services.Matches;
using PussyCats.Library.Services.PersonalityTestService;
using PussyCats.Library.Services.Skills;
using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.Users;
using PussyCats.Library.Services.CompanyRecommendationService;
using PussyCats.Library.Services.CompanyStatusService;
using PussyCats.Library.Services.CompatibilityService;

using PussyCats.Library.Services.CompletenessService;
using PussyCats.Library.Services.CooldownService;
using PussyCats.Library.Services.CvParsing;
using PussyCats.Library.Services.Developers;
using PussyCats.Library.Services.ImageStorage;
using PussyCats.Library.Services.JobSkills;
using PussyCats.Library.Services.Preferences;
using PussyCats.Library.Services.RecommendationAlgorithm;
using PussyCats.Library.Services.SkillGapService;

using PussyCats.Library.Services.SkillTests;
using PussyCats.Library.Services.UserProfileService;
using PussyCats.Library.Services.UserRecommendationService;

using PussyCats.Library.Services.UserSkillService;
using PussyCats.Library.Services.UserStatusService;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PussyCats_App;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private IServiceProvider serviceProvider = null!;

    public static Window? MainAppWindow { get; private set; }
    public static IServiceProvider Services => ((App)Current).serviceProvider;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
        UIDispatcher.Queue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        var services = new ServiceCollection();
        ConfigureServices(services);
        serviceProvider = services.BuildServiceProvider();
        AssertRepositoryProxyRegistrations();//todo: remove after removing all repoProxies
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
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


        //todo: delete repoProxies when all services have been replaced with serviceProxies
        //todo: until them, they are needed in other service calls
        RegisterRepositoryProxy<IUserRepository, UserRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IJobRepository, JobRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IMatchRepository, MatchRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<ICompanyRepository, CompanyRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IDocumentRepository, DocumentRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<ISkillRepository, SkillRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IJobSkillRepository, JobSkillRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IUserSkillRepository, UserSkillRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<ISkillGroupRepository, SkillGroupRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<ISkillTestRepository, SkillTestRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IPersonalityTestRepository, PersonalityTestRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IRecommendationRepository, RecommendationRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IChatRepository, ChatRepositoryProxy>(services, apiConfiguration);
        RegisterRepositoryProxy<IMessageRepository, MessageRepositoryProxy>(services, apiConfiguration);

        services.AddHttpClient<IFilesProxy, FilesProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl));

        RegisterServiceProxy<IChatService, ChatServiceProxy>(services, apiConfiguration);
        //services.AddSingleton<IDeveloperService, DeveloperService>();


        RegisterServiceProxy<ICompanyService,CompanyServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IPersonalityTestService, PersonalityTestServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ISkillTestService, SkillTestServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserProfileService, UserProfileServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ISkillService, SkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserService, UserServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompanyRecommendationService, CompanyRecommendationServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompatibilityService, CompatibilityServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<ICompanyStatusService, CompanyStatusServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IDocumentService, DocumentServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IImageStorageService, ImageStorageServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IJobService, JobServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IJobSkillService, JobSkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IMatchService, MatchServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IPreferenceService, PreferenceServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserRecommendationService, UserRecommendationServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<PussyCats.Library.Services.UserSkillService.IUserSkillService, UserSkillServiceProxy >(services, apiConfiguration);
        RegisterServiceProxy<IUserSkillService, UserSkillServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IUserStatusService, UserStatusServiceProxy>(services, apiConfiguration);
        RegisterServiceProxy<IDeveloperService, DeveloperServiceProxy>(services, apiConfiguration);


        //services.AddTransient<PussyCats.Library.Services.UserSkillService.IUserSkillService, PussyCats.Library.Services.UserSkillService.UserSkillService>();
        //services.AddTransient<ICompanyRecommendationService, CompanyRecommendationService>();
        // services.AddTransient<ICompanyStatusService, CompanyStatusService>();
        //services.AddTransient<ICompatibilityService, CompatibilityService>();
        services.AddTransient<ICompletenessService, CompletenessService>();
        services.AddTransient<ICooldownService>(provider => new CooldownService(
            provider.GetRequiredService<IRecommendationRepository>(),
            TimeSpan.FromHours(24)));
        services.AddTransient<ICvParsingService, CvParsingService>();
        //services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<ILocalDocumentFileService, DocumentService>();
       // services.AddTransient<IImageStorageService, PussyCats_App.Services.ImageStorageService.ImageStorageService>();
       // services.AddTransient<IJobService, JobService>();
       // services.AddTransient<IJobSkillService, JobSkillService>();
        RegisterServiceProxy<ILocalFileStorageService, FileStorageServiceProxy>(services, apiConfiguration);
       // services.AddTransient<IMatchService, MatchService>();
       // services.AddTransient<IPreferenceService, PreferenceService>();
        services.AddTransient<IRecommendationAlgorithm, RecommendationAlgorithm>();
       // services.AddTransient<PussyCats.Library.Services.UserSkillService.IUserSkillService, PussyCats.Library.Services.UserSkillService.UserSkillService>();
        services.AddTransient<ISkillGapService, SkillGapService>();
       // services.AddTransient<ISkillTestService, SkillTestService>();
       // services.AddTransient<IUserProfileService, UserProfileService>();
       // services.AddTransient<IUserRecommendationService, UserRecommendationService>();
        //services.AddTransient<IUserService, UserService>();
        //services.AddTransient<IUserSkillService, UserSkillService>();
       // services.AddTransient<IUserStatusService, UserStatusService>();

        // PdfExportService is view-scoped because it needs the page's WebView2 instance.
        RegisterViewModels(services);
    }

    private static void RegisterRepositoryProxy<TRepository, TProxy>(
        IServiceCollection services,
        ApiConfiguration apiConfiguration)
        where TRepository : class
        where TProxy : class, TRepository
    {
        services.AddHttpClient<TRepository, TProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl));
    }

    private static void RegisterServiceProxy<TService, TProxy>(
        IServiceCollection services,
        ApiConfiguration apiConfiguration)
        where TService : class
        where TProxy : class, TService
    {
        services.AddHttpClient<TService, TProxy>(client =>
            client.BaseAddress = new Uri(apiConfiguration.BaseUrl));
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

    private void AssertRepositoryProxyRegistrations()
    {
        var repositoryInterfaces = typeof(IUserRepository).Assembly
            .GetExportedTypes()
            .Where(type => type.IsInterface && type.Name.StartsWith("I", StringComparison.Ordinal) && type.Name.EndsWith("Repository", StringComparison.Ordinal));

        foreach (var repositoryInterface in repositoryInterfaces)
        {
            var implementation = serviceProvider.GetRequiredService(repositoryInterface);
            if (!implementation.GetType().Name.EndsWith("Proxy", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"DI violation: {repositoryInterface.Name} is not bound to a *Proxy implementation.");
            }
        }
    }
}
