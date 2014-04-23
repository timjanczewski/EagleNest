using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using RouteCalculator.Domain.Entities;
using RouteCalculator.Domain.Services;
using System.Device.Location;


namespace RouteCalculator.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            List<Vehicle> vehicles = GetVehicles();
            List<string> Makes = new List<string>();
            foreach (Vehicle v in vehicles)
            {
                Makes.Add(v.Make + " | " + v.Model);
            }

            ViewData["MakesModels"] = new SelectList(Makes);
            List<Quote> Quotes = new List<Quote>();
            Quote q = new Quote();
            Quotes.Add(q);
            ViewBag.Quotes = Quotes;

            return View();
        }

        public ActionResult GetPrice(string zipcode1, string zipcode2, string vehicle)
        {
            string Make = string.Empty;
            string Model = string.Empty;

            char[] delimiterChars = { '|' };
            string[] makemodel = vehicle.Split(delimiterChars);
            Make = makemodel[0].ToString();
            Model = makemodel[1].ToString();
           
            var origin = _db.ZipcodePools.Where(x => x.ZipCode == zipcode1);
            var destination = _db.ZipcodePools.Where(x => x.ZipCode == zipcode2);

            ZipcodePool pool1 = origin.FirstOrDefault();
            ZipcodePool pool2 = destination.FirstOrDefault();

            
            
            var Area1 = _db.MajorAreas.Where(x => x.Pool1Name == pool1.PoolName);
            MajorArea ma1 = Area1.FirstOrDefault();

            var Area2 = _db.MajorAreas.Where(x => x.Pool1Name == pool2.PoolName);
            MajorArea ma2 = Area2.FirstOrDefault();

            Quote q = new Quote();
            if (ma1 != null && ma2 != null)
            {

                var routes = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName);
                Route baseprice = routes.FirstOrDefault();

                double multiplier = 1 + GetVehicleMultiplier(Make, Model);
                double price = Convert.ToDouble(baseprice.Price) * multiplier + (150);

                q.Vehicle = Make + " " + Model;
                q.Area1 = zipcode1;
                q.Area2 = zipcode2;
                q.Price = (int)Math.Round(price, 1);
            }
            else
            {
                q.Vehicle = Make + " " + Model;
                q.Area1 = zipcode1;
                q.Area2 = zipcode2;
                q.Price = 0;
            }
            List<Vehicle> vehicles = GetVehicles();
            List<string> Makes = new List<string>();
            foreach (Vehicle v in vehicles)
            {
                Makes.Add(v.Make + " | " + v.Model);
            }

            ViewData["MakesModels"] = new SelectList(Makes);
            List<Quote> Quotes = new List<Quote>();
            Quotes.Add(q);
            ViewBag.Quotes = Quotes;

            return View("Index");
        }

        public List<Vehicle> GetVehicles()
        {

            var makes = _db.Vehicles;

            List<Vehicle> vehicles = new List<Vehicle>();
            vehicles = makes.ToList<Vehicle>();
                        
            
            return vehicles;
        }

        private double GetVehicleMultiplier(string make, string model)
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

        public ActionResult GetClosestMA(double oLat, double oLong)
        {
            List<ClosestMajorArea> list = new List<ClosestMajorArea>();

            var oCoord = new GeoCoordinate(oLat, oLong);

            var ma = _db.MajorAreas;

            foreach (MajorArea m in ma)
            {
                try
                {
                    var details = _db.ZipcodePoolsPivotPoints.Where(x => x.PoolName == m.Pool1Name && x.State == m.Pool1State);
                    ZipcodePoolsPivotPoint z = details.FirstOrDefault();

                    ClosestMajorArea cma = new ClosestMajorArea();
                    cma.AreaName = m.MainCityName;
                    cma.ZipCode = z.ZipCode;
                    cma.Latitude = z.Latitude;
                    cma.Longitude = z.Longitude;

                    var maCoord = new GeoCoordinate(cma.Latitude, cma.Longitude);

                    cma.Distance = (oCoord.GetDistanceTo(maCoord) * 3.28084) / 5280;

                    list.Add(cma);
                }
                catch { }
            }

            list = list.OrderBy(x => x.Distance).Take(3).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBearing()
        {
            rhumb r = new rhumb();

            //double bearing = r.GetBearing(45.464854, -98.4923, 29.198562, -96.272851);
            double bearing = r.DegreeBearing(45.464854, -98.4923, 29.198562, -96.272851);

            return Json(bearing, JsonRequestBehavior.AllowGet);
        }
        
        
    }
}
