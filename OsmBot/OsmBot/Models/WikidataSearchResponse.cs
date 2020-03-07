using System.Collections.Generic;

namespace OsmBot.Models
{
    public class WikidataSearchResponse
    {
        public List<WikidataEntry> WikidataEntries { get; }
        public bool AlreadyMatched { get; }
        
        
        
        public WikidataSearchResponse(
            List<WikidataEntry> wikidataEntries = null,
            bool alreadyMatched = false)
        {
            WikidataEntries = wikidataEntries;
            AlreadyMatched = alreadyMatched;
        }

    }
    
    public class WikidataEntry{
        public string Id { get; }
        public string Label { get; }
        public string Description { get; }
        public string InstanceOf { get; }

        public WikidataEntry(string id, string label, string description, string instanceOf)
        {
            Id = id;
            Label = label;
            Description = description;
            InstanceOf = instanceOf;
        }
    }
}