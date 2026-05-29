using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Repositories.Interfaces;
using Tests_and_Interviews.Services.Interfaces;
using Tests_and_Interviews.Models;
namespace Tests_and_Interviews.Services
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
