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
            XmlDocument gpxFile = new XmlDocument();
            List<LocationPoint> points = GetLocationPointsFromFile(gpxFile);

            // how accurate do we want to be? How many segments will we simplify this route into?
            List<LocationPoint> pointsToUse = GetSimplifiedSegmentPoints(points, 4);

            var segments = new List<ActivitySector>();

            // What way is the wind blowing? 0 = North
            Double windBearing = 180;
            Console.WriteLine("Wind: " + windBearing.ToString());

            for (int i = 0; i < pointsToUse.Count - 1; i++)
            {
                var newSegment = new ActivitySector(pointsToUse[i], pointsToUse[i + 1]);
                segments.Add(newSegment);

                Console.WriteLine("------------");
                Console.WriteLine("Bearing: " + newSegment.GetBearing());
                Console.WriteLine("Differance: " + newSegment.BearingCompare(windBearing).ToString());
                Console.WriteLine("Wind Perception: " + newSegment.PerceivedHeadwindAngle(windBearing));
            }
        }

        private static List<LocationPoint> GetSimplifiedSegmentPoints(List<LocationPoint> points, int desiredDivision)
        {
            var pointsToUse = new List<LocationPoint>();

            // first point
            pointsToUse.Add(points.First());

            // interim pointa
            for (decimal i = 1; i < desiredDivision; i++)
            {
                var fractionalIndex = (i / desiredDivision);
                var targetIndex = Convert.ToInt32(fractionalIndex * points.Count);

                pointsToUse.Add(points[targetIndex]);
            }

            // final point
            pointsToUse.Add(points.Last());
            return pointsToUse;
        }

        private static List<LocationPoint> GetLocationPointsFromFile(XmlDocument gpxFile)
        {
            gpxFile.Load("C:\\Users\\James\\source\\repos\\Sandbox\\Sandbox\\example1.gpx");

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

        public Double BearingCompare(Double windDirection)
        {
            var result = this._bearing - windDirection;
            return (result + 180) % 360 - 180;
        }

        public String PerceivedHeadwindAngle(Double windDirection)
        {
            var diff = this.BearingCompare(windDirection);
            bool positive = diff > 0;
            diff = Math.Abs(diff);
            var result = "";

            if (diff < 15)
            {
                result = "Strong Tailwind";
            }
            else if (diff < 45)
            {
                result = "Tailwind";
            }
            else if (diff < 135)
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
}
