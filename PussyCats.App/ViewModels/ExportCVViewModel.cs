using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PussyCats.App.Configuration;
using PussyCats_App.Services.PdfExportService;
using PussyCats.Library.Services.UserProfileService;

namespace PussyCats.App.ViewModels;

public partial class ExportCVViewModel : DispatchableObservableObject
{
    private readonly IUserProfileService userProfileService;
    private IPdfExportService? pdfExportService;
    private readonly SessionContext? session;
    private string statusText = string.Empty;
    private bool isLoading;

    public ExportCVViewModel(IUserProfileService userProfileService)
    {
        this.userProfileService = userProfileService;
    }

    public ExportCVViewModel(IUserProfileService userProfileService, SessionContext session)
        : this(userProfileService)
    {
        this.session = session;
    }

    public ExportCVViewModel(IPdfExportService pdfExportService, IUserProfileService userProfileService)
        : this(userProfileService)
    {
        this.pdfExportService = pdfExportService;
    }

    public int UserId { get; set; }

    public string StatusText
    {
        get => statusText;
        set => SetProperty(ref statusText, value);
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public void AttachExportService(IPdfExportService exportService)
    {
        pdfExportService = exportService;
    }

    public async Task LoadAndRenderCVAsync(CancellationToken cancellationToken = default)
    {
        if (pdfExportService is null)
        {
            StatusText = "PDF preview is not attached to this view.";
            return;
        }

        var resolvedUserId = UserId > 0
            ? UserId
            : session is null ? ViewModelSupport.DefaultUserId : ViewModelSupport.ResolveUserId(session);

        IsLoading = true;
        StatusText = "Loading CV preview...";

        try
        {
            var userProfile = await userProfileService.GetProfileAsync(resolvedUserId, cancellationToken)
                ?? throw new InvalidOperationException("User profile not found.");

            await pdfExportService.RenderProfileAsync(userProfile);
            StatusText = string.Empty;
        }
        catch (Exception exception)
        {
            StatusText = $"Preview failed: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExportCVAsync()
    {
        if (pdfExportService is null)
        {
            StatusText = "PDF export is not attached to this view.";
            return;
        }

        IsLoading = true;
        StatusText = "Saving PDF...";

        try
        {
            await pdfExportService.DownloadPdfAsync();
            StatusText = "Downloaded successfully!";
        }
        catch (OperationCanceledException)
        {
            StatusText = string.Empty;
        }
        catch (Exception exception)
        {
            StatusText = $"Export failed: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
