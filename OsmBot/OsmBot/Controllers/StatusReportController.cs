using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OsmBot.Models;

namespace OsmBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Gives some insight in the database
        /// </summary>
        [HttpGet]
        public ActionResult<StatusReport> Get(bool kill = false)
        {
            return new StatusReport();
        }
    }
}