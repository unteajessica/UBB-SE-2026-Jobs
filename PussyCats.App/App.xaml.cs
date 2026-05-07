using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PussyCats.App.Configuration;
using PussyCats.App.RepositoryProxies;
using PussyCats.App.Services;
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
using PussyCats.Library.Services;

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
        AssertRepositoryProxyRegistrations();
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

        services.AddTransient<IChatService, ChatService>();
        services.AddSingleton<IDeveloperService, DeveloperService>();

        services.AddTransient<ICompanyRecommendationService, CompanyRecommendationService>();
        services.AddTransient<ICompanyService, CompanyService>();
        services.AddTransient<ICompanyStatusService, CompanyStatusService>();
        services.AddTransient<ICompatibilityService, CompatibilityService>();
        services.AddTransient<ICompletenessService, CompletenessService>();
        services.AddTransient<ICooldownService>(provider => new CooldownService(
            provider.GetRequiredService<IRecommendationRepository>(),
            TimeSpan.FromHours(24)));
        services.AddTransient<ICvParsingService, CvParsingService>();
        services.AddTransient<IDocumentService, DocumentService>();
        services.AddTransient<IImageStorageService, ImageStorageService>();
        services.AddTransient<IJobService, JobService>();
        services.AddTransient<IJobSkillService, JobSkillService>();
        services.AddTransient<ILocalFileStorageService, LocalFileStorageService>();
        services.AddTransient<IMatchService, MatchService>();
        services.AddTransient<IPersonalityTestService, PersonalityTestService>();
        services.AddTransient<IPreferenceService, PreferenceService>();
        services.AddTransient<IRecommendationAlgorithm, RecommendationAlgorithm>();
        services.AddTransient<ISkillGapService, SkillGapService>();
        services.AddTransient<ISkillTestService, SkillTestService>();
        services.AddTransient<IUserProfileService, UserProfileService>();
        services.AddTransient<IUserRecommendationService, UserRecommendationService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IUserSkillService, UserSkillService>();
        services.AddTransient<IUserStatusService, UserStatusService>();

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
