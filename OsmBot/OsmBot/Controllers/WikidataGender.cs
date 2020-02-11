using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OsmBot.WikipediaTools;

namespace OsmBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class WikiDataGender : ControllerBase
    {
        /// <summary>
        /// Gives possible entries
        /// </summary>
        [HttpGet]
        public ActionResult<string> Get(string wikidata)
        {
            return WikipediaToWikidata.GetGender(wikidata);
        }
    }
}