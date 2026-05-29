using PussyCats.Library.Domain;
using PussyCats.Library.Services.CompanyService;

namespace PussyCats.App.ViewModels;

public class CompanyProfileViewModel : DispatchableObservableObject
{
    private readonly ICompanyService companyService;
    private Company? company;
    private string errorMessage = string.Empty;

    public CompanyProfileViewModel(ICompanyService companyService)
    {
        this.companyService = companyService;
    }

    public Company? Company
    {
        get => company;
        private set => SetProperty(ref company, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public async Task LoadAsync(int companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            Company = await companyService.GetByIdAsync(companyId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
    }
}
