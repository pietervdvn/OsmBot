using System;
using NetTopologySuite.Geometries;
using OsmBot.Download;

namespace OsmBot
{
    public static class Wgs84To900913
    {
        private const int _earthRadius = 6378137;
        private const double _originShift = 2 * Math.PI * _earthRadius / 2;

        //Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
        public static Point ConvertWgs84To900913(this Point p)
        {
            var lat = p.Y;
            var lon = p.X;
            var x = lon * _originShift / 180;
            var y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180);
            y = y * _originShift / 180;
            return new Point(x, y);
        }

        //Converts XY point from (Spherical) Web Mercator EPSG:3785 (unofficially EPSG:900913) to lat/lon in WGS84 Datum
        public static Point Convert900913ToWgs84(this Point p)
        {
            var x = 180 * p.X / _originShift;
            var y = 180 * p.Y / _originShift;
            y = 180 / Math.PI * (2 * Math.Atan(Math.Exp(y * Math.PI / 180)) - Math.PI / 2);
            return new Point(x, y);
        }

        public static Rect ConvertWgs84To900913(this Rect r)
        {
            return new Rect(
                ConvertWgs84To900913(r.Min),
                ConvertWgs84To900913(r.Max)
                );
        }
        
        public static Rect Convert900913ToWgs84(this Rect r)
        {
            return new Rect(
                Convert900913ToWgs84(r.Min),
                Convert900913ToWgs84(r.Max)
            );
        }

    }
}