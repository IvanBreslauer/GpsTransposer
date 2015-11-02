using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IvanB.GPSTransposer;
using IvanB.GPSTransposer.Models;
using System.IO;

namespace GPSTransposerTests
{
    [TestClass]
    public class GPSTransposerTest
    {
        [TestMethod]
        public void TestTranspose()
        {
            var p = new TransposerParams(
                new ImgPoint(219, 686, null),
                new ImgPoint(1167, 64, null),
                new GpsPoint(2.42463, 39.577310000000004, null),
                new GpsPoint(3.07493, 39.90439000000001, null),
                1000);

            string filePath = Environment.CurrentDirectory + "\\GpxData.txt";
            StreamReader streamReader = new StreamReader(filePath);
            string gpxContent = streamReader.ReadToEnd();
            streamReader.Close();

            var transposed = GPSTransposer.TransposeToJSON(p, gpxContent);
            
            Assert.IsNotNull(transposed);
        }
    }
}
