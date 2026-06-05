using PussyCats.Library.Domain;

namespace PussyCats.Tests.Fakes;

public class CompanyBuilder
{
    private int companyId = 1;
    private string companyName = "Acme Corp";
    private string logoText = "ACME";
    private string email = "hr@acme.test";
    private string phone = "+40000000000";

    public CompanyBuilder WithId(int id)
    {
        companyId = id;
        return this;
    }

    public CompanyBuilder WithName(string value)
    {
        companyName = value;
        return this;
    }

    public CompanyBuilder WithEmail(string value)
    {
        email = value;
        return this;
    }

    public CompanyBuilder WithPhone(string value)
    {
        phone = value;
        return this;
    }

    public Company Build() => new()
    {
        CompanyId = companyId,
        CompanyName = companyName,
        LogoText = logoText,
        Email = email,
        Phone = phone,
    };
}
