using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OsmBot.WikipediaTools
{
    /// <summary>
    /// Fetches the wikipedia tag, fetches the wikidata tag
    /// </summary>
    public class WikipediaToWikidata
    {
        static readonly HttpClient _client = new HttpClient();


        private static Dictionary<string, string> _instanceOf = new Dictionary<string, string>
        {
            {"Q5", "human"},
            {"Q860861", "sculpture"},
            {"Q79007", "street"},
            {"Q2977", "cathedral"},
            {"Q33506", "museum"},
            {"Q12280", "bridge"},
            {"Q174782", "square"},
            {"Q5633421", "scientific_journal"},
            {"Q11446", "ship"},
            {"Q4167410", "disambiguation_page"},
            {"Q1454597", "masonic_lodge"},
            {"Q83620", "thoroughfare"},
            {"Q42744322", "german_city"},
            {"Q868557", "music festival"},
            {"Q2785216", "municipality_section"},
            {"Q20643955", "biblical_figure"}
        };

        public static string GetInstance(Dictionary<string, string> statements)
        {
            if (!statements.ContainsKey("P31"))
            {
                return "";
            }

            var key = statements["P31"];
            if (_instanceOf.ContainsKey(key))
            {
                return _instanceOf[key];
            }

            try
            {
                var metaStatement = Statements(key).Result.label;
                Console.WriteLine("Trying to get statement " + key);
                _instanceOf.Add(key, metaStatement);
                Console.WriteLine("Instance https://www.wikidata.org/wiki/" + key + " turns out to be a " +
                                  metaStatement);
                return metaStatement;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

        public static async Task<(Dictionary<string, string>, string label)> Statements(string wikidata)
        {
            var url =
                $"https://www.wikidata.org/w/api.php?action=wbgetentities&ids={wikidata}&format=json";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await _client.GetStringAsync(url);

            var root = JObject.Parse(responseBody)["entities"][wikidata];
            var jsonClaims = root["claims"];

            var json = (JObject) jsonClaims;

            var claims = new Dictionary<string, string>();
            foreach (var kv in json)
            {
                try
                {
                    var key = kv.Key;


                    var value = kv.Value[0]["mainsnak"]["datavalue"]["value"]["id"].Value<string>();
                    claims[key] = value;
                }
                catch (ArgumentNullException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (NullReferenceException)
                {
                }
            }

            try
            {
                var labels = root["labels"];
                var label = "?";

                foreach (var lb in labels.Children())
                {
                    if (label.Equals("?") || lb.First["language"].Value<string>().Equals("en"))
                    {
                        label = lb.First["value"].Value<string>();
                    }
                }

                return (claims, label);
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("While getting " + wikidata + "\n" + e);
                return (claims, "?");
            }
        }

        public static Dictionary<string, string> _genderCache = new Dictionary<string, string>();

        public static string GetGender(string wikidata)
        {
            if (_genderCache.TryGetValue(wikidata, out var gender))
            {
                return gender;
            }

            return _genderCache[wikidata] = GetGenderRaw(wikidata).Result;
        }

        private static async Task<string> GetGenderRaw(string wikidata)
        {
            var statements = Statements(wikidata).Result.Item1;

            if (statements.TryGetValue("P21", out var gender))
            {
                if (gender.Equals("Q6581097"))
                {
                    return "male";
                }

                if (gender.Equals("Q6581072"))
                {
                    return "female";
                }
            }

            if (!statements.TryGetValue("P31", out var instance))
            {
                return "nonhuman";
            }

            if (!instance.Equals("Q5"))
            {
                return "nonhuman";
            }

            return "other";
        }

        public static async Task<List<Dictionary<string, string>>>
            SearchWikidata(string search, string language)
        {
            search = search.Trim('-');
            var url =
                "https://www.wikidata.org/w/api.php?action=wbsearchentities&format=json" +
                $"&search={search}&language={language}";

            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await _client.GetStringAsync(url);

            var jsonObj = JObject.Parse(responseBody)["search"];

            var json = (JArray) jsonObj;
            if (json.Count == 0)
            {
                Console.WriteLine("No entry found for " + search);
            }

            var entries = new List<Dictionary<string, string>>();
            foreach (var elem in json)
            {
                var values = new Dictionary<string, string>
                {
                    ["id"] = elem["id"].Value<string>(),
                    ["label"] = elem["label"].Value<string>(),
                    ["description"] = elem["description"]?.Value<string>()
                };
                var statements = (await Statements(values["id"])).Item1;
                values["instanceOf"] = GetInstance(statements);
                entries.Add(values);
            }

            return entries;
        }
    }
}