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
        public Service service = new Service();
        public MajorAreaRouteExit isFinal = new MajorAreaRouteExit();
        public string Destination;
        public string Passthrough;
        public ActionResult Index()
        {
            List<Vehicle> vehicles = service.GetVehicles();
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

            ZipcodePool start = _db.ZipcodePools.Where(x => x.ZipCode == zipcode1).FirstOrDefault();
            ZipcodePool end = _db.ZipcodePools.Where(x => x.ZipCode == zipcode2).FirstOrDefault();

            MajorArea ma1 = _db.MajorAreas.Where(x => x.Pool1Name == start.PoolName).FirstOrDefault();
            MajorArea ma2 = _db.MajorAreas.Where(x => x.Pool1Name == end.PoolName).FirstOrDefault();
            
            Quote q = new Quote();
            if (ma1 != null && ma2 != null)
            {
                Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();
                
                double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                double price = Convert.ToDouble(baseprice.Price) * multiplier + (150);

                q.Vehicle = Make + " " + Model;
                q.Area1 = zipcode1;
                q.Area2 = zipcode2;
                q.Price = (int)Math.Round(price, 1);
            }
            else
            {
                if (ma1 != null && ma2 == null)
                {
                    
                    ZipcodePoolsPivotPoint startPoint = _db.ZipcodePoolsPivotPoints.Where(x => x.PoolName == ma1.Pool1Name).FirstOrDefault(); 
                    ZipcodePoolsPivotPoint endPoint = _db.ZipcodePoolsPivotPoints.Where(x => x.ZipCode == end.ZipCode).FirstOrDefault(); 
                    
                    List<ClosestMajorArea> cma = GetClosestMA(endPoint.Latitude, endPoint.Longitude);

                    List<RouteRhumb> rr = new List<RouteRhumb>();
                    foreach (ClosestMajorArea c in cma)
                    {
                        RouteRhumb r = new RouteRhumb();

                        r.Zipcode1 = startPoint.ZipCode;
                        r.lat1 = startPoint.Latitude;
                        r.lon1 = startPoint.Longitude;
                        r.ZipCode2 = c.ZipCode;
                        r.lat2 = c.Latitude;
                        r.lon2 = c.Longitude;
                        r.Rhumb = service.GetRhumbLine(r.lat1, r.lon1, r.lat2, r.lon2);
                        r.Distance = c.Distance;

                        rr.Add(r);
                    }

                    RouteRhumb or = new RouteRhumb();
                    or.Zipcode1 = startPoint.ZipCode;
                    or.lat1 = startPoint.Latitude;
                    or.lon1 = startPoint.Longitude;
                    or.ZipCode2 = endPoint.ZipCode;
                    or.lat2 = endPoint.Latitude;
                    or.lon2 = endPoint.Longitude;
                    or.Rhumb = service.GetRhumbLine(or.lat1, or.lon1, or.lat2, or.lon2);

                    var oCoord = new GeoCoordinate(or.lat1, or.lon1);
                    var eCoord = new GeoCoordinate(or.lat2, or.lon2);

                    or.Distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;

                    ///pull interstates


                }

                if (ma2 != null && ma1 == null)
                { 
                    ZipcodePoolsPivotPoint oPoint2 = _db.ZipcodePoolsPivotPoints.Where(x => x.ZipCode == end.ZipCode).FirstOrDefault();
                    
                    List<ClosestMajorArea> cma = GetClosestMA(oPoint2.Latitude, oPoint2.Longitude);
                }
                
                
                
                
                q.Vehicle = Make + " " + Model;
                q.Area1 = zipcode1;
                q.Area2 = zipcode2;
                q.Price = 0;
            
                
            
            }
            
            
            
            
            
            List<Vehicle> vehicles = service.GetVehicles();
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

        public List<ClosestMajorArea> GetClosestMA(double oLat, double oLong)
        {
            List<ClosestMajorArea> list = new List<ClosestMajorArea>();

            var oCoord = new GeoCoordinate(oLat, oLong);

            var ma = _db.MajorAreas;

            foreach (MajorArea m in ma)
            {
                try
                {
                    ZipcodePoolsPivotPoint z = _db.ZipcodePoolsPivotPoints.Where(x => x.PoolName == m.Pool1Name && x.State == m.Pool1State).FirstOrDefault();
                    
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



            return list;
        }


        public ActionResult GetRouteExits(string ma1, string ma2)
        {
            int start = Convert.ToInt32(ma1.Replace("A", ""));
            int finish = Convert.ToInt32(ma2.Replace("A", ""));

            string Major1 = string.Empty;
            string Major2 = string.Empty;

            if (start < finish)
            { Major1 = ma1; Major2 = ma2; }
            else
            { Major1 = ma2; Major2 = ma1; }
            
            var exits = _db.MajorAreaRouteExits.Where(x => x.start == Major1 && x.finish == Major2 && x.RouteLevel != null).OrderBy(o1 => o1.RouteNumber).OrderBy(o2 => o2.RouteLevel); 

            List<MajorAreaRouteExit> maExits = new List<MajorAreaRouteExit>();

            foreach (MajorAreaRouteExit mae in exits)
            {
                MajorAreaRouteExit exit = new MajorAreaRouteExit();

                exit.Id = mae.Id;
                exit.start = mae.start;
                exit.finish = mae.finish;
                exit.RouteNumber = Convert.ToInt32(mae.RouteNumber);
                exit.RouteLevel = mae.RouteLevel;
                exit.state = mae.state;
                exit.interstate = mae.interstate;
                exit.Exit_from = mae.Exit_from;
                exit.Exit_to = mae.Exit_to;
                exit.PassThroughMA = mae.PassThroughMA;

                maExits.Add(exit);
            }
            
            if(Destination == null)
            { Destination = ma2; }

            if (Passthrough == null)
            { isFinal = maExits.Where(f => f.PassThroughMA == Destination).FirstOrDefault(); }
            else
            { isFinal = maExits.Where(f => f.PassThroughMA == Passthrough).FirstOrDefault(); }

            if (isFinal == null)
            {
                MajorAreaRouteExit cont = maExits.FirstOrDefault();
                int Area = Convert.ToInt32(Destination.Replace("A", ""));
                int Pass = Convert.ToInt32(cont.PassThroughMA.Replace("A", ""));

                Passthrough = cont.PassThroughMA;

                if (Area < Pass)
                { GetRouteExits(Destination, "A" + Pass.ToString()); }
                else
                { GetRouteExits("A" + Pass.ToString(), Destination); }
         

            }

            return Json(isFinal, JsonRequestBehavior.AllowGet);

        }

        //public Ac GetFinal(string ma1, string ma2)
        //{
        //    MajorAreaRouteExit isFinal = exits.Where(f => f.PassThroughMA == ma2).FirstOrDefault();

        //    if (isFinal != null)
        //    {
        //        MajorAreaRouteExit cont = maExits.FirstOrDefault();
        //        int newMA1 = Convert.ToInt32(ma2.Replace("A", ""));
        //        int newMA2 = Convert.ToInt32(cont.PassThroughMA.Replace("A", ""));

        //        if (newMA1 < newMA2)
        //        { GetRouteExits("A" + newMA1.ToString(), "A" + newMA2.ToString()); }
        //        else
        //        { GetRouteExits("A" + newMA2.ToString(), "A" + newMA1.ToString()); }


        //    }

        //}

        //public ActionResult GetFinalRoute(MajorAreaRouteExit exit)
        //{
        //    return Json(exit, JsonRequestBehavior.AllowGet);
        //}

        
        
    }
}
