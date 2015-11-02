using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IvanB.GPSTransposer.Models;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace IvanB.GPSTransposer
{
    public class Deserializer
    {
        public static List<GpsPoint> ToGpsList(string xml)
        {
            List<GpsPoint> mapPoints = new List<GpsPoint>();

            var serializer = new XmlSerializer(typeof(GPX));
            GPX parsedGpx;
            string parsedResult = string.Empty;

            using (TextReader reader = new StringReader(xml))
            {
                try
                {
                    parsedGpx = (GPX)serializer.Deserialize(reader);

                    if (parsedGpx != null && parsedGpx.Trk != null && parsedGpx.Trk.TrkSegList != null && parsedGpx.Trk.TrkSegList.Length > 0)
                    {
                        foreach (TrkSeg seg in parsedGpx.Trk.TrkSegList)
                            foreach (Trkpt point in seg.TrkPtList)
                            {
                                double d;
                                mapPoints.Add(new GpsPoint() { Lon = XmlConvert.ToDouble(point.Lon), Lat = XmlConvert.ToDouble(point.Lat), Elev = string.IsNullOrEmpty(point.Ele) ? 0 : XmlConvert.ToDouble(point.Ele) });
                            }
                    }
                }
                catch (Exception ex)
                {
                    // log here
                }
            }
            return mapPoints;
        }

        #region XML classes

        [XmlRoot(ElementName = "gpx", Namespace = "http://www.topografix.com/GPX/1/1", DataType = "string", IsNullable = true)]
        public class GPX
        {
            [XmlElement("trk")]
            public Trk Trk { get; set; }
        }

        public class Trk
        {
            [XmlElement("name")]
            public string Name { get; set; }

            [XmlElement("link")]
            public Link Link { get; set; }

            [XmlElement("trkseg")]
            public TrkSeg[] TrkSegList { get; set; }
        }

        public class Link
        {
            [XmlAttribute("href")]
            public string Href;
        }

        public class TrkSeg
        {
            [XmlElement("trkpt")]
            public Trkpt[] TrkPtList { get; set; }
        }

        public class Trkpt
        {
            [XmlAttribute(AttributeName = "lat")]
            public string Lat { get; set; }

            [XmlAttribute(AttributeName = "lon")]
            public string Lon { get; set; }

            [XmlElement(ElementName = "ele")]
            public string Ele { get; set; }
        }

        #endregion
    }
}
