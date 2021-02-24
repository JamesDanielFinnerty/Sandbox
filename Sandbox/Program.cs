using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using Sandbox.FSharp;
using Sandbox.Models;

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

            var routes = new List<RouteModel>();
            routes.Add(new RouteModel(gpxFile1, simplificationFactor));
            routes.Add(new RouteModel(gpxFile2, simplificationFactor));
            routes.Add(new RouteModel(gpxFile3, simplificationFactor));

            var days = new List<Day>();
            days.Add(new Day(new DateTime(2021, 02, 23), 270));
            days.Add(new Day(new DateTime(2021, 02, 24), 90));
            days.Add(new Day(new DateTime(2021, 02, 25), 120));
            days.Add(new Day(new DateTime(2021, 02, 26), 320));

            foreach(var day in days)
            {
                RouteModel bestRoute = routes
                    .OrderByDescending(x => x.PerceivedWindAffect(day.GetWind()))
                    .Last();

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
