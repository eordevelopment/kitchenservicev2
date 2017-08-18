using Microsoft.AspNetCore.Mvc;

namespace KitchenServiceV2.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        // GET: api/status
        [HttpGet]
        public string Get()
        {
            //this._logger.LogInformation("Application running. Status returning ok");
            return "running";
        }
    }
}
