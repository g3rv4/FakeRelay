using FakeRelay.Core;
using FakeRelay.Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace FakeRelay.Web.Controllers;

public class AcitivityPubController : Controller
{
   
    [Route("actor")]
    public ActionResult Actor() =>
        Content(MastodonHelper.GetActorStaticContent(), "application/activity+json; charset=utf-8");
    
    [Route("inbox"), HttpPost]
    public async Task<ActionResult> Inbox([FromBody] ActivityPubModel model)
    {
        if (model.Type == "Follow")
        {
            await MastodonHelper.ProcessInstanceFollowAsync(model);
        }

        return Content("{}", "application/activity+json");
    }

    [Route(".well-known/webfinger")]
    public ActionResult WebFinger(string resource)
    {
        if (resource == "acct:relay@" + Config.Instance.Host)
        {
            return Content(MastodonHelper.GetActorWebFinger(), "application/json; charset=utf-8");
        }

        return new ContentResult
        {
            Content = @"{""error"": ""user not found""}",
            ContentType = "application/json; charset=utf-8",
            StatusCode = 404
        };
    }
}
