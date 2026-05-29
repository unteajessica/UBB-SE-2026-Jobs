namespace Tests_and_Interviews_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using Tests_and_Interviews_API.Dtos;
    using Tests_and_Interviews_API.Mappers;
    using Tests_and_Interviews_API.Models;
    using Tests_and_Interviews_API.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class CollaboratorsController : ControllerBase
    {
        private readonly ICollaboratorsService _service;

        public CollaboratorsController(ICollaboratorsService service)
        {
            this._service = service;
        }

        [HttpPost]
        public ActionResult AddCollaborator([FromBody] CollaboratorDto collaboratorDto, [FromQuery] int loggedInUserID)
        {
            Event eventOfCollaboration = collaboratorDto.EventId > 0
                ? new Event { Id = collaboratorDto.EventId }
                : new Event();

            Company collaboratorToBeAdded = new Company { CompanyId = collaboratorDto.CompanyId };

            this._service.AddCollaboratorToRepo(eventOfCollaboration, collaboratorToBeAdded, loggedInUserID);

            return Ok();
        }

        [HttpGet("{loggedInCompanyId}")]
        public ActionResult<List<CompanyDto>> GetAllCollaborators(int loggedInCompanyId)
        {
            List<Company> collaborators = this._service.GetAllCollaborators(loggedInCompanyId);

            if (collaborators is null || !collaborators.Any())
                return NotFound($"No collaborators found for company ID {loggedInCompanyId}.");

            return Ok(collaborators.Select(c => c.ToDto()).ToList());
        }
    }
}