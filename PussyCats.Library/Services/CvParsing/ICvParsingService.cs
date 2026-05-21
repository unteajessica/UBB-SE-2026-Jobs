using PussyCats.Library.Domain;

namespace PussyCats.Library.Services.CvParsing;

public interface ICvParsingService
{
    User ParseCvFile(string content, string fileType);
}
