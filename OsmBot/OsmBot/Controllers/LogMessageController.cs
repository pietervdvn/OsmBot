using System;
using Microsoft.AspNetCore.Mvc;
using OsmBot.Models;

namespace OsmBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class LogMessageController : ControllerBase
    {
        /// <summary>
        /// Gives some insight in the database
        /// </summary>
        [HttpGet]
        public void Get(string msg)
        { 
            var ip = HttpContext.Connection.RemoteIpAddress;
            var pth = $"log-{DateTime.Now.Date:s}.log.txt";
            System.IO.File.AppendAllText(pth, $"{{\"date\": {DateTime.Now:s}, \"ip\":{ip}, \"msg\":{msg}}}\n");
        }
    }
}