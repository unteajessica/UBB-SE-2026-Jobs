using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews_API.Repositories.Interfaces;
using Tests_and_Interviews_API.Services.Interfaces;
using Tests_and_Interviews_API.Models;
namespace Tests_and_Interviews_API.Services
{
    public class SessionService
    {
        public Company LoggedInUser { get; }

        public SessionService(Company user)
        {
            this.LoggedInUser = user;
        }
    }
}
