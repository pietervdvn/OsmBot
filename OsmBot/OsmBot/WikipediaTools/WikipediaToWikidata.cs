using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OsmBot.Download;
using OsmSharp.Complete;

namespace OsmBot.WikipediaTools
{
    /// <summary>
    /// Fetches the wikipedia tag, fetches the wikidata tag
    /// </summary>
    public class WikipediaToWikidata
    {
        static readonly HttpClient _client = new HttpClient();

        private EasyChangeset _cs;

        public WikipediaToWikidata(EasyChangeset cs)
        {
            _cs = cs;
        }

        public bool ShouldBeAStreet(string wikipedia, string label, string description)
        {
            if (wikipedia.EndsWith(" (Brugge)"))
            {
                wikipedia = wikipedia.Substring(0, wikipedia.Length - 9);
            }

            if (!wikipedia.ToLower().Equals(label.ToLower()))
            {
                return true;
            }

            if (description.ToLower().Equals("street in bruges, belgium"))
            {
                return false;
            }

            if (description.Equals("square in Bruges, Belgium"))
            {
                return false;
            }

            return true;
        }

        private static bool AlwaysSuspicious(string wikipedia, string label, string description)
        {
            return true;
        }


        public void AddWikidata(ICompleteOsmGeo geo)
        {
            if (geo.Tags == null)
            {
                return;
            }

            var wikipedia = geo.Tags.GetValue("wikipedia");
            if (string.IsNullOrEmpty(wikipedia))
            {
                return;
            }

            var alreadyExistingWikidata = geo.Tags.GetValue("wikidata");
            var wikidata = CachedFetch(wikipedia, alreadyExistingWikidata, AlwaysSuspicious);
            if (string.IsNullOrEmpty(wikidata))
            {
                // Console.WriteLine("BEWARE: no wikidata found for " + geo);
                return;
            }

            geo.AddNewTag("wikidata", wikidata);
            _cs.AddChange(geo);
        }

        private Dictionary<string, string> _cache = new Dictionary<string, string>();

        private string CachedFetch(string wikipedia, string alreadyExistingWikidata,
            Func<string, string, string, bool> isSuspicious)
        {
            if (_cache.ContainsKey(wikipedia))
            {
                return _cache[wikipedia];
            }


            Console.Write($"\r{_cache.Count:000}\t");
            try
            {
                return _cache[wikipedia] = FetchWikidata(wikipedia, alreadyExistingWikidata, isSuspicious).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR for {wikipedia}: {e.Message}");
                return null;
            }
        }

        private async Task<string> FetchWikidata(string wikipedia, string alreadyExistingWikidata,
            Func<string, string, string, bool> isSuspicious)
        {
            var split = wikipedia.Split(":");
            if (split.Length != 2)
            {
                Console.WriteLine("Invalid wikipedia tag: " + wikipedia);
                return null;
            }

            var language = split[0];
            var title = split[1];
            var url =
                "https://www.wikidata.org/w/api.php?action=wbgetentities&format=json&formatversion=2&languages=en&origin=*" +
                $"&sites={language}wiki&titles={title}";

            var response = await _client.GetAsync("http://www.contoso.com/");
            response.EnsureSuccessStatusCode();
            var responseBody = await _client.GetStringAsync(url);

            var json = (JObject) JObject.Parse(responseBody)["entities"];

            if (json.Count > 1)
            {
                Console.WriteLine("Ambigous occurence for " + wikipedia);
                return null;
            }

            if (json.Count == 0)
            {
                Console.WriteLine("No entry found for " + wikipedia);
            }


            foreach (var kv in json)
            {
                var q = kv.Key;

                if (q.Equals(alreadyExistingWikidata))
                {
                    return q;
                }

                try
                {
                    var labels = kv.Value["labels"]["en"]["value"].Value<string>();
                    var descr = kv.Value["descriptions"]["en"]["value"].Value<string>();
                    Console.Write($"\r{wikipedia} --> {labels}; {descr}                          ");
                    if (isSuspicious(title, labels, descr))
                    {
                        Console.WriteLine($"\r{wikipedia} has to be checked: description is {descr}");
                    }
                }
                catch (NullReferenceException)
                {
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nERROR: {e.Message} for {wikipedia}");
                    return null;
                }

                return q;
            }

            return null;
        }
    }
}