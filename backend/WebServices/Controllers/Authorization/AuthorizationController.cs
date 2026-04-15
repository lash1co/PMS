using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebServices.Controllers.Authorization
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAuthorizations()
        {
            return Ok(new
            {
                Message = "This is a protected resource. You are authorized to access it."
            });
        }
    }
}
