using PussyCats.Library.Domain;

namespace PussyCats_App.Services.CvParsingService;

public interface ICvParsingService
{
    User ParseCvFile(string content, string fileType);
}
