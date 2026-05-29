using System.Collections.Generic;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface ICollaboratorsService
    {
        Task AddCollaborator(Event eventToBeCollaboratedOn, Company companyInvitedToCollaborate, int loggedInUserID);
        Task<List<Company>> GetAllCollaborators(int loggedInCompanyId);
    }
}