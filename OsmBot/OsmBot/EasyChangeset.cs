using System;
using System.Collections.Generic;
using System.IO;
using OsmSharp;
using OsmSharp.Changesets;
using OsmSharp.Complete;
using OsmSharp.IO.Xml;
using OsmSharp.Streams;

namespace OsmBot
{
    public class EasyChangeset
    {
        private List<OsmGeo> modified = new List<OsmGeo>();
        private List<ICompleteOsmGeo> modifiedComplete = new List<ICompleteOsmGeo>();


        public void AddChange(ICompleteOsmGeo geo)
        {
            modifiedComplete.Add(geo);
            switch (geo)
            {
                case CompleteWay w:
                    modified.Add(w.ToSimple());
                    break;
                case Node n:
                    modified.Add(n);
                    break;
                case CompleteRelation r:
                    modified.Add(r.ToSimple());
                    break;
                default:
                    throw new Exception($"Unknown type: {geo}");
            }
        }

        public void WriteChangeTo(string path)
        {
            File.WriteAllText(path, ToChange().SerializeToXml());
        }

        public OsmChange ToChange()
        {
            return new OsmChange
            {
                Version = 0.6,
                Create = new OsmGeo[0],
                Delete = new OsmGeo[0],
                Generator = "Pietervdvn's little OSM bot",
                Modify = modified.ToArray()
            };
        }


        public void WriteOsmTo(string path)
        {
            using (var outStream = File.OpenWrite(path))
            {
                var outstream = new XmlOsmStreamTarget(outStream);
                foreach (var m in modifiedComplete)
                {
                    switch (m)
                    {
                        case CompleteWay w:

                            foreach (var n in w.Nodes)
                            {
                                outstream.AddNode(n);
                            }

                            outstream.AddWay((Way) w.ToSimple());
                            break;
                        case Node n:
                            modified.Add(n);
                            break;
                        case CompleteRelation r:
                            throw new Exception($"Unknown type: {r}");
                    }
                }
            }
        }
    }
}