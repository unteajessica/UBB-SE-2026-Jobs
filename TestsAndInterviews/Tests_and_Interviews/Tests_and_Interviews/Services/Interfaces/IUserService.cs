using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests_and_Interviews.Models.Core;

namespace Tests_and_Interviews.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
    }
}
