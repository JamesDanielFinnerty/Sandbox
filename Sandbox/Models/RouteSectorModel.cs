using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sandbox.Models
{
    class RouteSectorModel
    {
        LocationPoint _start;
        LocationPoint _end;
        Double _bearing;

        public RouteSectorModel(LocationPoint start, LocationPoint end)
        {
            this._start = start;
            this._end = end;

            var deltaLongitude = ToRadians(_end.GetLongitude() - _start.GetLongitude());

            var deltaPhi = Math.Log(Math.Tan(ToRadians(_end.GetLatitude()) / 2 + Math.PI / 4) / Math.Tan(ToRadians(_start.GetLatitude()) / 2 + Math.PI / 4));

            if (Math.Abs(deltaLongitude) > Math.PI)
            {
                if (deltaLongitude > 0)
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
}
