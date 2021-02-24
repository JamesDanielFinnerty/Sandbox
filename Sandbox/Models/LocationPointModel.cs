using System;
using System.Collections.Generic;
using System.Text;

namespace Sandbox.Models
{
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
}
