using FakeRelay.Core;
using Microsoft.AspNetCore.Mvc;

namespace FakeRelay.Web.Controllers;

public class HomeController : Controller
{
    [Route("")]
    public ActionResult Index()
    {
        if (Config.Instance.HomeRedirect != null)
        {
            return Redirect(Config.Instance.HomeRedirect);
        }

        return Content("Hi! I'm FakeRelay. You can learn more about me at fakerelay.gervas.io");
    }
}
