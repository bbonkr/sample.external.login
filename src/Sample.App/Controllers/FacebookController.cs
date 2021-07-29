using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Sample.App.Services;

namespace Sample.App.Controllers
{
    public class FacebookController : Controller
    {
        public FacebookController(
            UserManager<IdentityUser> userManager,
            FacebookService facebookService)
        {
            this.userManager = userManager;
            this.facebookService = facebookService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);

            if(user == null)
            {
                return NotFound();
            }

            var logins = await userManager.GetLoginsAsync(user);

            var facebookLogin = logins.Where(x => x.LoginProvider == FacebookDefaults.AuthenticationScheme).FirstOrDefault();

            return View(facebookLogin);
        }

        [HttpGet]
        public IActionResult Test()
        {
            var value = "<signed_request value here>";

            var data = facebookService.ParseSignedRequest(value);


            return Json(data);
        }

        [HttpGet]
        public IActionResult Deleted([FromQuery] string id)
        {
            var response = new { Id = id };
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserData()
        {
            var bodyStream = Request.Form["signed_request"].ToString();
            
            var data = facebookService.ParseSignedRequest(bodyStream);

            // -------------------------------------------------------
            // DELETE USER LOGIN
            // -------------------------------------------------------
            var user = await userManager.FindByLoginAsync(FacebookDefaults.AuthenticationScheme, data.UserId);
            if (user == null)
            {
                throw new Exception("Could not find the user data with provided data.");
            }

            // Remove user login
            var removeLoginResult = await userManager.RemoveLoginAsync(user, FacebookDefaults.AuthenticationScheme, data.UserId);

            if (!removeLoginResult.Succeeded)
            {
                throw new Exception($"Could not remove the user's login information. ({string.Join(", ", removeLoginResult.Errors.Select(e => e.Description))})");
            }

            // For test, delete user.
            var deleteResult = await userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                throw new Exception($"Could not remove the user. ({string.Join(", ", deleteResult.Errors.Select(e => e.Description))})");
            }
            // -------------------------------------------------------
            // DELETE USER LOGIN
            // -------------------------------------------------------

            var newRequestId = Guid.NewGuid().ToString();

            var response = new DeleteUserDataCallbackResponseModel
            {
                Url = $"{Request.Scheme}://{Request.Host}/facebook/deleted?id={newRequestId}",
                ConfirmationCode = newRequestId,
            };

            return Json(response);
        }
    

        private readonly UserManager<IdentityUser> userManager;
        private readonly FacebookService facebookService;
    }

    public class DeleteUserDataCallbackResponseModel
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("confirmation_code")]
        public string ConfirmationCode { get; set; }
    }
}
