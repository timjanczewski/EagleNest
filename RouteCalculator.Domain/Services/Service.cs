﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RouteCalculator.Domain.Entities;

namespace RouteCalculator.Domain.Services
{
    public class Service
    {
        public RouteDbContext _db = new RouteDbContext();


        /// <summary>
        /// Get Vehicle Information
        /// </summary>
        /// <returns></returns>
        public List<Vehicle> GetVehicles()
        {

            var makes = _db.Vehicles;

            List<Vehicle> vehicles = new List<Vehicle>();
            vehicles = makes.ToList<Vehicle>();


            return vehicles;
        }

        public double GetVehicleMultiplier(string make, string model)
        {
            double percent = 0;

            string vmake = make.Trim();
            string vmodel = model.Trim();

            var vehicle = _db.Vehicles.Where(x => x.Make == vmake && x.Model == vmodel);
            Vehicle v = vehicle.FirstOrDefault();

            string vClass = v.Category;

            var cat = _db.Categories.Where(c => c.Class == vClass);
            Category category = cat.FirstOrDefault();

            if (category.PricePercentIncrease > 0)
            { percent = Convert.ToDouble(category.PricePercentIncrease); }

            return percent;
        }

        ///<summary>
        /// Get Routing Information
        /// </summary>


        /// <summary>
        /// Get Mapping Geolocation Information
        /// </summary>
        /// <returns></returns>
        public double GetRhumbLine(double lat1, double lon1, double lat2, double lon2)
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


        public double GetBearing(double lat1, double lon1, double lat2, double lon2)
        {
            rhumb r = new rhumb();

            double bearing = GetRhumbLine(lat1, lon1, lat2, lon2);

            return bearing;
        }
    }
}
