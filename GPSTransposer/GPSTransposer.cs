using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jil;
using IvanB.GPSTransposer.Models;

namespace IvanB.GPSTransposer
{
    public class GPSTransposer
    {
        #region Properties

        /// <summary> List of GPS points extracted from a .GPX file or other similar source of GPS data </summary>
        public static List<GpsPoint> GpsPoints { get; set; }

        /// <summary> List of transposed points expressed in pixels (for computer graphic coordinate system usage) </summary>
        private static List<ImgPoint> _imgPoints = new List<ImgPoint>();

        /// <summary> Transposer parameters </summary>
        public static TransposerParams _params;

        // Helper fields
        private static double _maxLng;			// Max longitude
        private static double _maxLat;			// Max latitude
        private static double? _maxElev;		// Highest elevation point
        private static double? _minElev;		// Lowest elevation point
        private static double _scalingFactorX;	// Coefficient for horizontal scaling of the route
        private static double _scalingFactorY;	// Coefficient for vertical scaling of the route
        private static double _offsetX;		    // Offset for correct positioning of the starting point of the route (X)
        private static double _offsetY;		    // Offset for correct positioning of the starting point of the route (Y)
        //

        #endregion

        #region Factory methods

        /// <summary>
        /// Parses .GPX file content and transposes it using provided calibration parameters.
        /// </summary>
        /// <param name="p">Calibration parameters used for transposing the points.</param>
        /// <param name="gpxFileContent">The string content of a .GPX file.</param>
        /// <returns>List of point objects.</returns>
        public static List<ImgPoint> TransposeToList(TransposerParams p, string gpxFileContent)
        {
            GpsPoints = Deserializer.ToGpsList(gpxFileContent);
            return Transpose(p, GpsPoints);
        }

        /// <summary>
        /// Parses .GPX file content and transposes it using provided calibration parameters.
        /// </summary>
        /// <param name="p">Calibration parameters used for transposing the points.</param>
        /// <param name="gpxFileContent">The string content of a .GPX file.</param>
        /// <returns>List of point objects as a JSON string.</returns>
        public static string TransposeToJSON(TransposerParams p, string gpxFileContent)
        {
            GpsPoints = Deserializer.ToGpsList(gpxFileContent);
            Transpose(p, GpsPoints);

            return JSON.Serialize<List<ImgPoint>>(_imgPoints);
        }

        private static List<ImgPoint> Transpose(TransposerParams p, List<GpsPoint> pts)
        {
            _params = p;

            if (pts.Count > 0)
            {
                _maxLng = pts.Min(pl => pl.Lon);
                _maxLat = pts.Max(pl => pl.Lat);

                CalculateFactorAndOffset();
                PointDistanceOptimization();
            }

            return _imgPoints;
        }

        #endregion

        #region Helper methods
                
        /// <summary>
        /// Calculates the scaling factor of a route as well as x/y offset for correct points positioning
        /// </summary>
		private static void CalculateFactorAndOffset()
		{
			GpsPoint firstGpsPoint = new GpsPoint(_params.GpsStartPoint.Lon, _params.GpsStartPoint.Lat, null);
            firstGpsPoint = TransposePoint(firstGpsPoint);
			
			GpsPoint lastGpsPoint = new GpsPoint (_params.GpsEndPoint.Lon, _params.GpsEndPoint.Lat, null);
            lastGpsPoint = TransposePoint(lastGpsPoint);
						
			_scalingFactorX = Math.Abs(_params.ImgStartPoint.X - _params.ImgEndPoint.X) / Math.Abs(firstGpsPoint.Lon - lastGpsPoint.Lon);
			_scalingFactorY = Math.Abs(_params.ImgStartPoint.Y - _params.ImgEndPoint.Y) / Math.Abs(firstGpsPoint.Lat - lastGpsPoint.Lat);

            firstGpsPoint = DislocatePointByFactor(firstGpsPoint);

            _offsetX = _params.ImgStartPoint.X > firstGpsPoint.Lon ? _params.ImgStartPoint.X - firstGpsPoint.Lon : firstGpsPoint.Lon - _params.ImgStartPoint.X;
            _offsetY = _params.ImgStartPoint.Y > firstGpsPoint.Lat ? _params.ImgStartPoint.Y - firstGpsPoint.Lat : firstGpsPoint.Lat - _params.ImgStartPoint.Y;
		}
		
		/// <summary>
		/// Transposes a GPS point into a coordinate system point
		/// </summary>
		/// <param name="point">GPS point</param>
		/// <returns>Transposed point for coordinate system</returns>
        private static GpsPoint TransposePoint(GpsPoint point)
		{			
			double factorX = _maxLng < 0 ? Math.Abs(_maxLng) : _maxLng * -1;
			double factorY = _maxLat < 0 ? Math.Abs(_maxLat) : _maxLat * -1;
			
			point.Lon += factorX;
			point.Lat = Math.Abs(point.Lat + factorY);

            return point;
		}
		
		// pomnoži koordinate točke s koeficijentom za ispravnu veličinu rute (scaling)

        /// <summary>
        /// Corrects the position of the point by a calculated offset
        /// </summary>
        /// <param name="point">Input point</param>
        /// <returns>Correctly positioned point after offset is applied</returns>
        private static GpsPoint DislocatePointByFactor(GpsPoint point)
		{			
			point.Lon *= _scalingFactorX;
			point.Lat *= _scalingFactorY;

            return point;
        }

        /// <summary>
        /// Optimizes the distance between points so that too dense and too sparse areas of points are evened-out.
        /// </summary>
		private static void PointDistanceOptimization()
		{
			GpsPoint lastPushed = new GpsPoint();
			GpsPoint current = new GpsPoint();
			
			for(int i = 0; i < GpsPoints.Count; i++)
			{
				current = TransposePoint(GpsPoints[i]);
				current = DislocatePointByFactor(current);
				current.Lon = Math.Abs(current.Lon) + _offsetX;
				current.Lat = Math.Abs(current.Lat) + _offsetY;
				
				// check if this is the last point of the list to optimize, if yes then add it and break the loop
				if (Math.Sqrt(Math.Pow(current.Lon - _params.ImgEndPoint.X, 2) + Math.Pow(current.Lat - _params.ImgEndPoint.Y, 2)) < _params.StepLength / 1000)
				{
					if (i == GpsPoints.Count - 1)
					{
						current.Lon = _params.ImgEndPoint.X;
						current.Lat = _params.ImgEndPoint.Y;
						_imgPoints.Add(new ImgPoint(current.Lon, current.Lat, current.Elev));
						break;
					}
				}
					
				if (i == 0)
				{
					lastPushed = current;
					_imgPoints.Add(new ImgPoint(lastPushed.Lon, lastPushed.Lat, lastPushed.Elev));
				}
				else
				{					
					double distPx = Math.Sqrt(Math.Pow(current.Lon - lastPushed.Lon, 2) + Math.Pow(current.Lat - lastPushed.Lat, 2));   // distance between the current and the preceeding point
                    double angle = Math.Acos(Math.Abs(current.Lon - lastPushed.Lon) / distPx);  // angle between the current and the preceeding point
					double distPxRnded = Math.Round(distPx * 1000);
					
					double offsetX = Math.Cos(angle) * (_params.StepLength / 1000);
					double offsetY = Math.Sin(angle) * (_params.StepLength / 1000);
                    GpsPoint filler = new GpsPoint();

                    if (distPxRnded >= _params.StepLength && distPxRnded < 2 * _params.StepLength)
					{
                        // acceptable point distance, add the point to the list
						lastPushed = current;
                        _imgPoints.Add(new ImgPoint(lastPushed.Lon, lastPushed.Lat, lastPushed.Elev));
					}
                    else if (distPxRnded >= 2 * _params.StepLength)
					{
						// add additional points to fill up too scarse area of points
						if (lastPushed.Lon == current.Lon)	// if the points are on the same vertical line
						{
							filler.Lon = lastPushed.Lon;
                            filler.Lat = lastPushed.Lat < current.Lat ? lastPushed.Lat + _params.StepLength / 1000 : lastPushed.Lat - _params.StepLength / 1000;
							filler.Elev = lastPushed.Elev;
							
							lastPushed = filler;
							_imgPoints.Add(new ImgPoint(lastPushed.Lon, lastPushed.Lat, lastPushed.Elev));
						}
                        else if (lastPushed.Lat == current.Lat)  // if the points are on the same horizontal line
						{
							filler.Lat = lastPushed.Lat;
                            filler.Lon = lastPushed.Lon < current.Lon ? lastPushed.Lon + _params.StepLength / 1000 : lastPushed.Lon - _params.StepLength / 1000;
							filler.Elev = lastPushed.Elev;
							
							lastPushed = filler;
							_imgPoints.Add(new ImgPoint(lastPushed.Lon, lastPushed.Lat, lastPushed.Elev));
						}
						else
						{
                            // calculate and add the "filler" point
							do
							{
								filler.Lon = lastPushed.Lon < current.Lon ? lastPushed.Lon + offsetX : lastPushed.Lon - offsetX;
								filler.Lat = lastPushed.Lat < current.Lat ? lastPushed.Lat + offsetY : lastPushed.Lat - offsetY;
								filler.Elev = lastPushed.Elev;
								
								lastPushed = filler;
								_imgPoints.Add(new ImgPoint(lastPushed.Lon, lastPushed.Lat, lastPushed.Elev));
								
								distPx = Math.Sqrt(Math.Pow(current.Lon - lastPushed.Lon, 2) + Math.Pow(current.Lat - lastPushed.Lat, 2));
								distPxRnded = Math.Round(distPx * 1000);
                            } while (distPxRnded >= 2 * _params.StepLength);
						}						
					}
				}
			}
		}

        #endregion
    }

    public class TransposerParams
    {
        public TransposerParams(ImgPoint imgA, ImgPoint imgB, GpsPoint gpsA, GpsPoint gpsB, int step)
        {
            _imgStartPoint = imgA;
            _imgEndPoint = imgB;
            _gpsStartPoint = gpsA;
            _gpsEndPoint = gpsB;
            _stepLength = step;
        }

        private ImgPoint _imgStartPoint = new ImgPoint();
        /// <summary>First point expressed in pixels, used for scaling and position calibration.</summary>
        public ImgPoint ImgStartPoint { get { return _imgStartPoint; } }

        private ImgPoint _imgEndPoint = new ImgPoint();
        /// <summary>Last point expressed in pixels, used for scaling and position calibration.</summary>
        public ImgPoint ImgEndPoint { get { return _imgEndPoint; } }

        private int _stepLength = 1000;
        /// <summary>Distance between two points, expressed in one thousanth of a pixel.</summary>
        public int StepLength { get { return _stepLength; } }

        private GpsPoint _gpsStartPoint = new GpsPoint();
        /// <summary>First point expressed in GPS coordinates, used for scaling and position calibration.</summary>
        public GpsPoint GpsStartPoint { get { return _gpsStartPoint; } }

        private GpsPoint _gpsEndPoint = new GpsPoint();
        /// <summary>Last point expressed in GPS coordinates, used for scaling and position calibration.</summary>
        public GpsPoint GpsEndPoint { get { return _gpsEndPoint; } }
    }
}
