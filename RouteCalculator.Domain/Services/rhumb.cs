using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Services
{
    public class rhumb
    {

        public double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; //earth’s radius (mean radius = 6,371km)
            var dLon = ToRad(lon2 - lon1);
            var dPhi = Math.Log(
                Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI)
                dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        public void GetRhumb()
        {
            //RouteRhumb or = new RouteRhumb();
            //or.Zipcode1 = startPoint.ZipCode;
            //or.lat1 = startPoint.Latitude;
            //or.lon1 = startPoint.Longitude;
            //or.ZipCode2 = endPoint.ZIPCode;
            //or.lat2 = endPoint.Latitude;
            //or.lon2 = endPoint.Longitude;
            //or.Rhumb = service.GetRhumbLine(or.lat1, or.lon1, or.lat2, or.lon2);

            //var oCoord = new GeoCoordinate(or.lat1, or.lon1);
            //var eCoord = new GeoCoordinate(or.lat2, or.lon2);

            //or.Distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;
        }

    }
}
