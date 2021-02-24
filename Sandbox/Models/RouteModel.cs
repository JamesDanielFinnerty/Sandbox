using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Sandbox.Models
{
    class RouteModel
    {
        List<LocationPoint> _locationPoints;
        List<LocationPoint> _simpleLocationPoints;
        List<RouteSectorModel> _sectors;
        int _simplificationFactor;
        string _name;

        public RouteModel(XmlDocument gpxFile, int simplificationFactor)
        {
            this._name = gpxFile.GetElementsByTagName("name")[0].InnerText;

            this._locationPoints = GetLocationPointsFromFile(gpxFile);
            this._simplificationFactor = simplificationFactor;
            this._simpleLocationPoints = GetSimplifiedSegmentPoints();

            this._sectors = new List<RouteSectorModel>();

            for (int i = 0; i < this._simpleLocationPoints.Count - 1; i++)
            {
                var newSegment = new RouteSectorModel(this._simpleLocationPoints[i], this._simpleLocationPoints[i + 1]);
                _sectors.Add(newSegment);
            }
        }

        public List<RouteSectorModel> GetSectors()
        {
            return _sectors;
        }

        public string GetName()
        {
            return _name;
        }

        public Double PerceivedWindAffect(Double windBearing)
        {
            Double result = 0;

            for (int i = 0; i < _sectors.Count(); i++)
            {
                Double score = _sectors[i].PerceivedEffort(windBearing);
                Double adjustedScore = score * (i + 1);
                result += adjustedScore;
            }

            return result;
        }

        private List<LocationPoint> GetSimplifiedSegmentPoints()
        {
            var pointsToUse = new List<LocationPoint>();
            var points = this._locationPoints;

            // first point
            pointsToUse.Add(points.First());

            // interim pointa
            for (decimal i = 1; i < this._simplificationFactor; i++)
            {
                var fractionalIndex = (i / this._simplificationFactor);
                var targetIndex = Convert.ToInt32(fractionalIndex * points.Count);

                pointsToUse.Add(points[targetIndex]);
            }

            // final point
            pointsToUse.Add(points.Last());
            return pointsToUse;
        }

        private List<LocationPoint> GetLocationPointsFromFile(XmlDocument gpxFile)
        {

            // Grab all the elements where the GPS coords are stored.
            // This might be Garmin specific
            XmlNodeList activityPoints = gpxFile.GetElementsByTagName("trkpt");


            var points = new List<LocationPoint>();

            for (int i = 0; i < activityPoints.Count; i++)
            {
                var lat = activityPoints[i].Attributes["lat"].Value;
                var lon = activityPoints[i].Attributes["lon"].Value;

                points.Add(new LocationPoint(lat, lon));
            }

            return points;
        }
    }
}
