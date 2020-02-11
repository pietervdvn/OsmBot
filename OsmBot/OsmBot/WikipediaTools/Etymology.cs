using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OsmBot.WikipediaTools
{
    public class Etymology
    {
        public static Etymology Singleton = new Etymology();

        private static List<string> _endings = new List<string>
        {
            "straat", "dreef", "laan", "steenweg", "Steenweg", "plein", "plantsoen", "rei", "reitje", "heerweg",
            "Heerweg", "weg", "kerkhof", "brug"
        };


        public static string DropStreetOfName(string streetName)
        {
            streetName = streetName.Replace("-", " ");
            foreach (var ending in _endings)
            {
                if (streetName.EndsWith(ending))
                {
                    return streetName.Substring(0, streetName.Length - ending.Length);
                }
            }

            return streetName;
        }

        private Dictionary<string, List<Dictionary<string, string>>> _cache =
            new Dictionary<string, List<Dictionary<string, string>>>();


        private List<string> _whiteList = new List<string>
        {
            "human", "municipality_section", "biblical_figure"
        };

        private List<string> _blacklist = new List<string>
        {
            "street", "sculpture", "disambiguation_page", "thoroughfare", "street", "painting", "Wikimedia human name disambiguation page"
        };

        /// <summary>
        /// Gives all the possible wikidata entries for the given object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<List<(string id, string label, string description, string instanceOf)>> FetchEtymologyFor(
            string name)
        {
            if (!_cache.TryGetValue(name, out var entries))
            {
                entries = (await WikipediaToWikidata.SearchWikidata(name, "nl"))
                    .Where(statements => !_blacklist.Contains(statements["instanceOf"]))
                    .ToList();
                if (entries.Count > 0)
                {
                    _cache[name] = entries;
                }
            }

            Console.WriteLine(name + " --> \n   " + string.Join("\n   ", entries.Select(Print)));

            Console.WriteLine("\n\n\n");
            return entries.Select(entry => (entry["id"], entry["label"], entry["description"], entry["instanceOf"]))
                .ToList();
        }

        private string Print(Dictionary<string, string> statements)
        {
            var txt = "";
            foreach (var kv in statements)
            {
                txt += kv.Key + "=" + kv.Value + " ";
            }

            return txt;
        }
    }
}