using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PussyCats.Web.Clients;
using PussyCats.Web.Models;

namespace PussyCats.Web.Controllers
{
    [Authorize(Policy = "AuthenticatedUser")]
    public class ProfileController : Controller
    {
        private readonly UsersApiClient usersApiClient;

        public ProfileController(UsersApiClient usersApiClient)
        {
            this.usersApiClient = usersApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var user = await this.usersApiClient.GetCurrentUser();

            if (user == null)
            {
                return this.Challenge();
            }

            ProfileViewModel viewModel = new ProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CvXml = user.CvXml,
            };

            return this.View(viewModel);
        }
    }
}
