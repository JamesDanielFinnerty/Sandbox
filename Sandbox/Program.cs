using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using Sandbox.FSharp;

namespace Sandbox.CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestFSharpCall();

            // Load GPX file, hardcoded path for now.
            XmlDocument gpxFile1 = new XmlDocument();
            gpxFile1.Load("C:\\Users\\James\\source\\repos\\Sandbox\\Sandbox\\example1.gpx");
            XmlDocument gpxFile2 = new XmlDocument();
            gpxFile2.Load("C:\\Users\\James\\source\\repos\\Sandbox\\Sandbox\\example2.gpx");
            XmlDocument gpxFile3 = new XmlDocument();
            gpxFile3.Load("C:\\Users\\James\\source\\repos\\Sandbox\\Sandbox\\example3.gpx");

            var simplificationFactor = 100;

            var routes = new List<Activity>();
            routes.Add(new Activity(gpxFile1, simplificationFactor));
            routes.Add(new Activity(gpxFile2, simplificationFactor));
            routes.Add(new Activity(gpxFile3, simplificationFactor));

            var days = new List<Day>();
            days.Add(new Day(new DateTime(2021, 02, 23), 270));
            days.Add(new Day(new DateTime(2021, 02, 24), 90));
            days.Add(new Day(new DateTime(2021, 02, 25), 120));
            days.Add(new Day(new DateTime(2021, 02, 26), 320));

            foreach(var day in days)
            {
                Activity bestRoute = routes[0];

                foreach(var activity in routes)
                {
                    if(activity.PerceivedWindAffect(day.GetWind()) < bestRoute.PerceivedWindAffect(day.GetWind()))
                    {
                        bestRoute = activity;
                    }
                }

                Console.WriteLine("On " + day.GetDate().Date.ToShortDateString() + " you should ride to " + bestRoute.GetName());
            }


        }

        

        

        private static void TestFSharpCall()
        {
            var answer = FSharp.testFunctions.factorial(5);

            Console.WriteLine("C Sharp Main Called");
            Console.WriteLine("FSharp Factorial 5: " + answer.ToString());

            //Call to get variable from FSharp, not working
            //var x = FSharp.testFunctions.results;
            //Console.WriteLine(x);
        }
    }

    class LocationPoint
    {
        // simple class to store GPS points
        Double _lat;
        Double _lon;
        public LocationPoint(string lat, string lon)
        {
            this._lat = Double.Parse(lat);
            this._lon = Double.Parse(lon);
        }

        public Double GetLongitude()
        {
            return this._lon;
        }

        public Double GetLatitude()
        {
            return this._lat;
        }
    }

    class Activity
    {
        List<LocationPoint> _locationPoints;
        List<LocationPoint> _simpleLocationPoints;
        List<ActivitySector> _sectors;
        int _simplificationFactor;
        string _name;

        public Activity(XmlDocument gpxFile, int simplificationFactor)
        {
            this._name = gpxFile.GetElementsByTagName("name")[0].InnerText;

            this._locationPoints = GetLocationPointsFromFile(gpxFile);
            this._simplificationFactor = simplificationFactor;
            this._simpleLocationPoints = GetSimplifiedSegmentPoints();

            this._sectors = new List<ActivitySector>();

            for (int i = 0; i < this._simpleLocationPoints.Count - 1; i++)
            {
                var newSegment = new ActivitySector(this._simpleLocationPoints[i], this._simpleLocationPoints[i + 1]);
                _sectors.Add(newSegment);
            }
        }

        public List<ActivitySector> GetSectors()
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

            for(int i = 0; i < _sectors.Count(); i++)
            {
                Double score = _sectors[i].PerceivedEffort(windBearing);
                Double adjustedScore = score * (i+1);
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

    class ActivitySector
    {
        LocationPoint _start;
        LocationPoint _end;
        Double _bearing;
        
        public ActivitySector(LocationPoint start, LocationPoint end)
        {
            this._start = start;
            this._end = end;

            var deltaLongitude = ToRadians(_end.GetLongitude() - _start.GetLongitude());

            var deltaPhi = Math.Log(Math.Tan(ToRadians(_end.GetLatitude()) / 2 + Math.PI / 4) / Math.Tan(ToRadians(_start.GetLatitude()) / 2 + Math.PI / 4));

            if (Math.Abs(deltaLongitude) > Math.PI)
            {
                if(deltaLongitude > 0)
                {
                    deltaLongitude = -(2 * Math.PI - deltaLongitude);
                }
                else
                {
                    deltaLongitude = (2 * Math.PI + deltaLongitude);
                }
            }

            this._bearing = ToBearing(Math.Atan2(deltaLongitude, deltaPhi));
        }

        public String GetBearing()
        {
            return this._bearing.ToString();
        }

        private Double myMod(Double x, Double m)
        {
            // normal % function doing something weird with negatives
            return (x % m + m) % m;
        }

        public Double BearingCompare(Double windDirection)
        {
            // broken into lines as this code keeps being a problem and this aids debugging a little
            var result = this._bearing - windDirection;
            var q = result + 180;
            var w = myMod(q, 360);
            var e = w - 180;

            return e;
        }

        public int PerceivedEffort(Double windDirection)
        {
            var diff = this.BearingCompare(windDirection);
            diff = Math.Abs(diff);

            var result = 0;

            if (diff < 15)
            {
                result = 1;
            }
            else if (diff < 80)
            {
                result = 3;
            }
            else if (diff < 100)
            {
                result = 2;
            }
            else if (diff < 165)
            {
                result = 7;
            }
            else if (diff < 180)
            {
                result = 10;
            }

            return result;
        }

        public String PerceivedHeadwindAngle(Double windDirection)
        {
            var diff = this.BearingCompare(windDirection);
            bool positive = diff > 0;
            diff = Math.Abs(diff);
            var result = "";

            if (diff < 15)
            {
                result = "Tailwind";
            }
            else if (diff < 80)
            {
                result = "Tailwind with Crosswind";
            }
            else if (diff < 100)
            {
                if (positive)
                {
                    result = "Crosswind from Right";
                }
                else
                {
                    result = "Crosswind from Left";
                }
            }
            else if (diff < 165)
            {
                result = "Headwind with Crosswind";
            }
            else if (diff < 180)
            {
                result = "Headwind";
            }

            return result;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        private static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }
    }

    class Day
    {
        DateTime _date;
        Double _windBearing;

        public Day(DateTime date, Double wind)
        {
            this._date = date;
            this._windBearing = wind;
        }

        public DateTime GetDate() { return this._date; }
        public Double GetWind() { return this._windBearing; }
    }
}
