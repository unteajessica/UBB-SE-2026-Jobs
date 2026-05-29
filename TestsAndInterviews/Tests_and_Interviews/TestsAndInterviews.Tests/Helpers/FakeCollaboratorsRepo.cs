using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models;
using Tests_and_Interviews.Repositories;
using Tests_and_Interviews.Repositories.Interfaces;

namespace TestsAndInterviews.Tests.Helpers
{
    internal class FakeCollaboratorsRepo : ICollaboratorsRepo
    {
        public Event ReceivedEvent = null;
        public Company ReceivedCompany = null;
        public int ReceivedLoggedInUserId = -1;
        public List<Company> CollaboratorsToReturn = new List<Company>();

        public void AddCollaboratorToRepo(Event eventOfCollaboration, Company collaboratorToBeAdded, int loggedInUserID)
        {
            ReceivedEvent = eventOfCollaboration;
            ReceivedCompany = collaboratorToBeAdded;
            ReceivedLoggedInUserId = loggedInUserID;
        }

        public List<Company> GetAllCollaborators(int loggedInCompanyId)
        {
            return CollaboratorsToReturn;
        }
    }
}
