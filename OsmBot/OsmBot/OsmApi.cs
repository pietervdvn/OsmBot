using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using OsmSharp;
using OsmSharp.IO.API;
using OsmSharp.Tags;

namespace OsmBot
{
    public class OsmApi
    {
        private INonAuthClient _client;
        public static OsmApi Singleton = new OsmApi();

        public OsmApi()
        {
            var clientFactory = new ClientsFactory(null, new HttpClient(),
                "https://www.openstreetmap.org/api/");
            _client = clientFactory.CreateNonAuthClient();
        }
        
        

        public async Task<OsmGeo> Get(string id)
        {
            var idL = long.Parse(id.Split("/")[1]);
            var type = id.Split("/")[0];
            if (type.Equals("way"))
            {
                return _client.GetWay(idL).Result;
            }

            if (type.Equals("node"))
            {
                return _client.GetNode(idL).Result;
            }

            if (type.Equals("relation"))
            {
                return _client.GetRelation(idL).Result;
            }

            throw new Exception("Unknown type " + type);
        }

        public static async Task CreateChange(IEnumerable<OsmGeo> objectsToUpdate)
        {
            var clientFactory = new ClientsFactory(null, new HttpClient(),
                "https://www.openstreetmap.org/api/");

            var client = clientFactory.CreateBasicAuthClient("EtymologyWikidataBot", Passwords.WikidataBotPassword);
            var changeSetTags = new TagsCollection
            {
                new Tag("comment", "Add name:etymology:wikidata"),
                new Tag("created_by", "PieterVdvn's OSM BOT")
            };
            var changeSetId = await client.CreateChangeset(changeSetTags);
            foreach (var obj in objectsToUpdate)
            {
                obj.Version = await client.UpdateElement(changeSetId, obj);
            }

            await client.CloseChangeset(changeSetId);
        }
    }
}