using System;
using System.Collections.Generic;
using OsmSharp;
using OsmSharp.Complete;

namespace Wikipedia2Street
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Adds the given tag to the given object, iff that tag did not exist yet.
        /// Emits a warning if the tag already exists and has a different value
        /// </summary>
        /// <param name="geo"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddNewTag(this ICompleteOsmGeo geo, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Key or value are either null or whitespace");
            }

            var oldValue = geo.Tags.GetValue(key);

            if (!string.IsNullOrEmpty(oldValue))
            {
                if (!oldValue.ToLower().Equals(value.ToLower()))
                {
                    // Hmm, our mechanical edit would like to change something - lets warn for this; human interaction is needed
                    throw new ArgumentException($"Warning for object {geo}: not overriding tag {key}={oldValue} with {value}");
                }

                // The tag already exists! We don't do anything
                return;
            }
            geo.Tags.Add(key, value);
        }
        
        
        
        public static EasyChangeset AddWikipediaTagsToStreets(this IEnumerable<ICompleteOsmGeo> completeSource)
        {
            var cs = new EasyChangeset();
            var wikiOnStreet = new WikipediaForBruges(cs);
            completeSource.OnEveryUnclosedWay(wikiOnStreet.ApplyWikipediaTag);
            return cs;
        }
        public static EasyChangeset AddWikidataToAll(this IEnumerable<ICompleteOsmGeo> completeSource)
        {
            var cs = new EasyChangeset();
            var wikiOnStreet = new WikipediaToWikidata(cs);
            completeSource.OnEveryObject(wikiOnStreet.AddWikidata);
            return cs;
        }
        public static void OnEveryObject(this IEnumerable<ICompleteOsmGeo> completeSource,
            Action<ICompleteOsmGeo> action)
        {
            foreach (var osmGeo in completeSource)
            {
                action(osmGeo);
            }
        }

        public static void OnEveryUnclosedWay(this IEnumerable<ICompleteOsmGeo> completeSource,
            Action<CompleteWay> action)
        {
            completeSource.OnEveryObject(
                osmGeo =>
                {
                    if (osmGeo.Type != OsmGeoType.Way)
                    {
                        return;
                    }

                    if (!(osmGeo is CompleteWay way))
                    {
                        return;
                    }

                    if (way.IsClosed())
                    {
                        return;
                    }

                    action(way);
                });
        }
    }
}