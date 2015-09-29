//using HNHB.Models;
using HNHB.Models.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace HNHB.HelperExtension
{
    public class CoordinateExtension
    {
        public static Coordinate GetCoordinates(string address)
        {
            WebRequest request = WebRequest.Create("https://maps.googleapis.com/maps/api/geocode/xml?key=AIzaSyC99lxB7ew2Zx9FaBA4xNW3nV6kaJX1ISs&address="
         + HttpUtility.UrlEncode(address));

            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    XDocument document = XDocument.Load(new StreamReader(stream));

                    XElement longitudeElement = document.Descendants("lng").FirstOrDefault();
                    XElement latitudeElement = document.Descendants("lat").FirstOrDefault();

                    if (longitudeElement != null && latitudeElement != null)
                    {
                        return new Coordinate()
                        {
                            Latitude = Convert.ToDouble(latitudeElement.Value, CultureInfo.InvariantCulture),
                            Longitude = Convert.ToDouble(longitudeElement.Value, CultureInfo.InvariantCulture)
                        };
                    }
                }
            }

            return null;
        }
    }
}