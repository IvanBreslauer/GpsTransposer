using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvanB.GPSTransposer.Models
{
    /// <summary>
    /// Class representing cartesian coordinate system point, transposed from a GPS point. 
    /// Holds the X and Y coordinates (with X values incrementing from left to right and Y values incrementing from top to bottom) and Elevation data
    /// </summary>
	public class ImgPoint 
	{
        /// <summary>X value (longitude), expressed in pixels</summary>
		public double X { get; set;}
        /// <summary>Y value (latitude), expressed in pixels</summary>
		public double Y { get; set;}
        /// <summary>Current elevation, taken from GPS data</summary>
		public double? Elev { get; set;}

        public ImgPoint() { }

		public ImgPoint (double x, double y, double? elev)
		{
			this.X = x;
			this.Y = y;
			this.Elev = elev;
		}		
	}

    /// <summary>
    /// Class representing a GPS point. 
    /// Holds the Logitude, Latitude and Elevation data.
    /// </summary>
    public class GpsPoint
    {
        /// <summary>Longitude, expressed in pixels</summary>
        public double Lon { get; set; }
        /// <summary>Latitude, expressed in pixels</summary>
        public double Lat { get; set; }
        /// <summary>Current elevation, taken from GPS data</summary>
        public double? Elev { get; set; }

        public GpsPoint() { }

        public GpsPoint(double lon, double lat, double? elev)
        {
            this.Lon = lon;
            this.Lat = lat;
            this.Elev = elev;
        }
    }
}
