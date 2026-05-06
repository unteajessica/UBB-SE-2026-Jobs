using Microsoft.AspNetCore.Mvc;

namespace PussyCats.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    // Phase 5 implements file storage. Both endpoints stub until then.

    [HttpGet("{id}")]
    public IActionResult GetFile(string id) =>
        Problem("File serving routes through /api/files in Phase 5.", statusCode: 501);

    [HttpPost]
    public IActionResult UploadFile() =>
        Problem("File upload routes through /api/files in Phase 5.", statusCode: 501);
}
