using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using RouteCalculator.Domain.Entities;
using RouteCalculator.Domain.Services;
using System.Device.Location;
using System.Text.RegularExpressions;


namespace RouteCalculator.Web.Controllers
{
    public class HomeController : BaseController
    {
        public Service service = new Service();
        public MajorAreaRouteExit isFinal = new MajorAreaRouteExit();
        public string Destination;
        public string Passthrough;

        public StringBuilder sb = new StringBuilder();

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

        public ActionResult GetPrice(string address1, string address2, string vehicle)
        {
            USZipcode startPoint = GetUSZipcode(address1);
            USZipcode endPoint = GetUSZipcode(address2);

            if (startPoint == null || endPoint == null)
            {
                ViewBag.Message = "Please enter a valid TO: and FROM: Address";

                List<Vehicle> vehicles2 = service.GetVehicles();
                List<string> Makes2 = new List<string>();
                foreach (Vehicle v2 in vehicles2)
                {
                    Makes2.Add(v2.Make + " | " + v2.Model);
                }

                ViewData["MakesModels"] = new SelectList(Makes2);
                List<Quote> Quotes2 = new List<Quote>();
                Quote q2 = new Quote();
                Quotes2.Add(q2);
                ViewBag.Quotes = Quotes2;

                return View("Index");
            }
            
            string Make = string.Empty;
            string Model = string.Empty;

            char[] delimiterChars = { '|' };
            string[] makemodel = vehicle.Split(delimiterChars);
            Make = makemodel[0].ToString();
            Model = makemodel[1].ToString();

            ZipcodePool start = _db.ZipcodePools.Where(x => x.ZipCode == startPoint.ZIPCode).FirstOrDefault();
            ZipcodePool end = _db.ZipcodePools.Where(x => x.ZipCode == endPoint.ZIPCode).FirstOrDefault();

            MajorArea ma1 = _db.MajorAreas.Where(x => x.Pool1Name == start.PoolName).FirstOrDefault();
            MajorArea ma2 = _db.MajorAreas.Where(x => x.Pool1Name == end.PoolName).FirstOrDefault();
            
            Quote q = new Quote();
            
            //GET DISTANCE FROM START TO END POINT
            double distance = service.DrivingDistance(startPoint.ZIPCode, endPoint.ZIPCode);

            //IF DISTANCE IS LESS THAN OR EQUAL TO 200 
            if (distance <= 200)
            {
                double basePrice = 0;
                if (ma1 != null && ma2 != null && (ma1 != ma2))
                {
                    Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();
                    basePrice = Convert.ToDouble(baseprice.Price);
                    sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                    sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                }
                else
                {
                    basePrice = distance * .8;
                    sb.AppendLine("Distance between points <= 200 miles.<br /> ");
                }
                
                if (basePrice < 100)
                { basePrice = 100; }

                double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                double price = Convert.ToDouble(basePrice) * multiplier + (150);

                q.Vehicle = Make + " " + Model;
                q.Area1 = address1;
                q.Area2 = address2;
                q.Price = (int)Math.Round(price, 1);

                
                sb.AppendLine("Base Price: " + basePrice + "<br /> ");
                sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                sb.AppendLine("Broker Fee: 150 <br /> ");
                sb.AppendLine("Quote: " + basePrice + " * " + multiplier + " + 150(Broker Fee) = " + q.Price);

                ViewBag.Log = sb.ToString();
            }
            
            //IF DISTANCE IS GREATER THAN 200
            else
            {
                if (ma1 != null && ma2 != null)
                {
                    if (ma1 == ma2)
                    {
                        double Distance = service.DrivingDistance(startPoint.ZIPCode, endPoint.ZIPCode);

                        double basePrice = Distance * .8;

                        if (basePrice < 100)
                        { basePrice = 100; }

                        double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                        double price = Convert.ToDouble(basePrice) * multiplier + (150);

                        q.Vehicle = Make + " " + Model;
                        q.Area1 = address1;
                        q.Area2 = address2;
                        q.Price = (int)Math.Round(price, 1);

                        sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                        sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                        sb.AppendLine("Base Price: " + basePrice + "<br /> ");
                        sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                        sb.AppendLine("Broker Fee: 150 <br /> ");
                        sb.AppendLine("Quote: " + basePrice + " * " + multiplier + " + 150(Broker Fee) = " + q.Price);

                        ViewBag.Log = sb.ToString();
                    }
                    else
                    {
                        double startdiff = 0;
                        if (GetDistanceToMAover30(startPoint, ma1) > 30)
                        {
                            MajorAreaRouteExit exit = new MajorAreaRouteExit();
                            exit = GetRouteExits(ma1.AreaName, ma2.AreaName, 2);

                            ClosestExit ce = GetClosestExit(exit, startPoint.Latitude, startPoint.Longitude);

                            if (ce.Distance > 30)
                            {
                                if (ce.Distance - 30 > 10)
                                {
                                    startdiff = ce.Distance - 30;
                                }
                            }

                        }
                        double enddiff = 0;
                        if (GetDistanceToMAover30(endPoint, ma2) > 30)
                        {
                            MajorAreaRouteExit exit = new MajorAreaRouteExit();
                            exit = GetRouteExits(ma1.AreaName, ma2.AreaName, 2);

                            ClosestExit ce = GetClosestExit(exit, endPoint.Latitude, endPoint.Longitude);

                            if (ce.Distance > 30)
                            {
                                if (ce.Distance - 30 > 10)
                                {
                                    enddiff = ce.Distance - 30;
                                }
                            }
                        }


                        Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();

                        double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);

                        double startdistance = startdiff * 2 * multiplier;
                        double enddistance = enddiff * 2 * multiplier;
                        double price = Convert.ToDouble(baseprice.Price) * multiplier + startdistance + enddistance + (150);

                        q.Vehicle = Make + " " + Model;
                        q.Area1 = address1;
                        q.Area2 = address2;
                        q.Price = (int)Math.Round(price, 1);

                        sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                        sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                        sb.AppendLine("Base Price: " + baseprice.Price + "<br /> ");
                        sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                        sb.AppendLine("Broker Fee: 150 <br /> ");
                        sb.AppendLine("OffRoute Major Area 1 Distance:" + startdiff + " * 2 * " + multiplier +" = " + startdistance + "<br /> ");
                        sb.AppendLine("OffRoute Major Area 2 Distance:" + enddiff + " * 2 * " + multiplier + " = " + enddistance + "<br /> ");
                        sb.AppendLine("Quote: " + baseprice.Price + " * " + multiplier + " + " + startdistance + " + " + enddistance + " + 150(Broker Fee) = " + q.Price);

                        ViewBag.Log = sb.ToString();
                    }
                }
                else
                {
                    if (ma1 != null && ma2 == null)
                    {

                        List<ClosestMajorArea> cma = GetClosestMA(endPoint.Latitude, endPoint.Longitude, ma1);

                        List<MajorAreaRouteExit> mare = new List<MajorAreaRouteExit>();

                        
                        //Get Major Area Exits//
                        foreach (ClosestMajorArea c in cma)
                        {

                            MajorAreaRouteExit exit = new MajorAreaRouteExit();

                            exit = GetRouteExits(ma1.AreaName, c.AreaName, 2);
                            mare.Add(exit);

                        }

                        List<ClosestExit> closestexits = new List<ClosestExit>();
                        foreach (MajorAreaRouteExit mae in mare)
                        {
                            closestexits.Add(GetClosestExit(mae, endPoint.Latitude, endPoint.Longitude));
                        }

                        for (int i = 0; i < cma.Count(); i++)
                        {

                            cma[i].ClosestExitPointDistance = closestexits[i].Distance;

                        }

                        ClosestMajorArea final = (ClosestMajorArea)cma.OrderBy(x => x.ClosestExitPointDistance).FirstOrDefault();

                        ma2 = _db.MajorAreas.Where(x => x.AreaName == final.AreaName).FirstOrDefault();

                        //ma1 distance to start
                        double startdiff = 0;
                        if (GetDistanceToMAover30(startPoint, ma1) > 30)
                        {
                            MajorAreaRouteExit exit = new MajorAreaRouteExit();
                            exit = GetRouteExits(ma1.AreaName, ma2.AreaName, 2);

                            ClosestExit ce = GetClosestExit(exit, startPoint.Latitude, startPoint.Longitude);

                            if (ce.Distance > 30)
                            {
                                if (ce.Distance - 30 > 10)
                                {
                                    startdiff = ce.Distance - 30;
                                }
                            }

                        }
                        
                        
                        double offroute = 0;

                        if (final.ClosestExitPointDistance > 6 & final.ClosestExitPointDistance <= 50)
                        { offroute = 50; }
                        else if (final.ClosestExitPointDistance > 50 & final.ClosestExitPointDistance <= 100)
                        { offroute = 100; }
                        else if (final.ClosestExitPointDistance > 100)
                        { offroute = 150; }



                        Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();

                        double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                        double startdistance = startdiff * 2 * multiplier;
                        double price = (Convert.ToDouble(baseprice.Price) + offroute) * multiplier + startdistance + (150);

                        q.Vehicle = Make + " " + Model;
                        q.Area1 = address1;
                        q.Area2 = address2;
                        q.Price = (int)Math.Round(price, 1);


                        sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                        sb.AppendLine("OffRoute Area 2: Closest Major Areas - " + cma[0].AreaName + "," + cma[1].AreaName + "," + cma[2].AreaName + "<br /> ");
                        sb.AppendLine("Closest Major Area Exit - Interstate: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Interstate + " Junction:" + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Junction + " Distance: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance + "<br /> ");
                        sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                        sb.AppendLine("Base Price: " + baseprice.Price + "<br /> ");
                        sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                        sb.AppendLine("Broker Fee: 150 <br /> ");
                        sb.AppendLine("OffRoute Major Area 1 Distance:" + startdiff + " * 2 * " + multiplier + " = " + startdistance + "<br /> ");
                        sb.AppendLine("OffRoute * Multiplier: " + (offroute * multiplier) + "<br /> ");
                        sb.AppendLine("Quote: " + baseprice.Price + " * " + multiplier + " + " + startdistance + " + 150(Broker Fee) + " + (offroute * multiplier) + " = " + q.Price);

                        ViewBag.Log = sb.ToString();

                    }

                    if (ma2 != null && ma1 == null)
                    {

                        List<ClosestMajorArea> cma = GetClosestMA(startPoint.Latitude, startPoint.Longitude, ma2);

                        List<MajorAreaRouteExit> mare = new List<MajorAreaRouteExit>();

                        foreach (ClosestMajorArea c in cma)
                        {

                            MajorAreaRouteExit exit = new MajorAreaRouteExit();

                            exit = GetRouteExits(c.AreaName, ma2.AreaName, 1);
                            mare.Add(exit);
                        }

                        List<ClosestExit> closestexits = new List<ClosestExit>();
                        foreach (MajorAreaRouteExit mae in mare)
                        {
                            closestexits.Add(GetClosestExit(mae, startPoint.Latitude, startPoint.Longitude));
                        }

                        for (int i = 0; i < cma.Count(); i++)
                        {
                            cma[i].ClosestExitPointDistance = closestexits[i].Distance;
                        }

                        ClosestMajorArea final = cma.OrderBy(x => x.ClosestExitPointDistance).Take(1).FirstOrDefault();

                        ma1 = _db.MajorAreas.Where(x => x.AreaName == final.AreaName).FirstOrDefault();

                        //ma2 distance to endpoint
                        double enddiff = 0;
                        if (GetDistanceToMAover30(endPoint, ma2) > 30)
                        {
                            MajorAreaRouteExit exit = new MajorAreaRouteExit();
                            exit = GetRouteExits(ma1.AreaName, ma2.AreaName, 2);

                            ClosestExit ce = GetClosestExit(exit, endPoint.Latitude, endPoint.Longitude);

                            if (ce.Distance > 30)
                            {
                                if (ce.Distance - 30 > 10)
                                {
                                    enddiff = ce.Distance - 30;
                                }
                            }
                        }

                        double offroute = 0;

                        if (final.ClosestExitPointDistance > 6 & final.ClosestExitPointDistance <= 50)
                        { offroute = 50; }
                        else if (final.ClosestExitPointDistance > 50 & final.ClosestExitPointDistance <= 100)
                        { offroute = 100; }
                        else if (final.ClosestExitPointDistance > 100)
                        { offroute = 150; }

                        Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();

                        double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                        double enddistance = enddiff * 2 * multiplier;
                        double price = Convert.ToDouble(baseprice.Price) * multiplier + (150) + (offroute * multiplier);

                        q.Vehicle = Make + " " + Model;
                        q.Area1 = address1;
                        q.Area2 = address2;
                        q.Price = (int)Math.Round(price, 1);

                        sb.AppendLine("OffRoute Area 1: Closest Major Areas - " + cma[0].AreaName + "," + cma[1].AreaName + "," + cma[2].AreaName + "<br /> ");
                        sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                        sb.AppendLine("Closest Major Area Exit - Interstate: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Interstate + " Junction:" + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Junction + " Distance: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance + "<br /> ");
                        sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                        sb.AppendLine("Base Price: " + baseprice.Price + "<br /> ");
                        sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                        sb.AppendLine("Broker Fee: 150 <br /> ");
                        sb.AppendLine("OffRoute Major Area 2 Distance:" + enddiff + " * 2 * " + multiplier + " = " + enddistance + "<br /> ");
                        sb.AppendLine("OffRoute * Multiplier: " + (offroute * multiplier) + "<br /> ");
                        sb.AppendLine("Quote: " + baseprice.Price + " * " + multiplier + " + " + enddistance + " + 150(Broker Fee) + " + (offroute * multiplier) + " = " + q.Price);

                        ViewBag.Log = sb.ToString();
                    }

                    if (ma1 == null && ma2 == null)
                    {

                        MajorArea blank = new MajorArea();
                        List<ClosestMajorArea> cma1 = GetClosestMA(startPoint.Latitude, startPoint.Longitude, blank);
                        List<ClosestMajorArea> cma2 = GetClosestMA(endPoint.Latitude, endPoint.Longitude, cma1);

                        List<MajorAreaRouteExit> mare1 = new List<MajorAreaRouteExit>();
                        List<MajorAreaRouteExit> mare2 = new List<MajorAreaRouteExit>();

                        foreach (ClosestMajorArea c1 in cma1)
                        {

                            MajorAreaRouteExit exit = new MajorAreaRouteExit();

                            foreach (ClosestMajorArea c2 in cma2)
                            {
                                exit = GetRouteExits(c1.AreaName, c2.AreaName, 0);
                                mare1.Add(exit);
                            }
                        }

                        List<ClosestExit> closestexits = new List<ClosestExit>();
                        foreach (MajorAreaRouteExit mae in mare1)
                        {
                            closestexits.Add(GetClosestExit(mae, startPoint.Latitude, startPoint.Longitude));
                        }


                        List<ClosestExit> ce1 = new List<ClosestExit>();
                        List<ClosestExit> ce2 = new List<ClosestExit>();
                        List<ClosestExit> ce3 = new List<ClosestExit>();

                        ce1.Add(closestexits[0]);
                        ce1.Add(closestexits[1]);
                        ce1.Add(closestexits[2]);

                        ce2.Add(closestexits[3]);
                        ce2.Add(closestexits[4]);
                        ce2.Add(closestexits[5]);

                        ce3.Add(closestexits[6]);
                        ce3.Add(closestexits[7]);
                        ce3.Add(closestexits[8]);

                        cma1[0].ClosestExitPointDistance = ce1.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance;
                        cma1[1].ClosestExitPointDistance = ce2.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance;
                        cma1[2].ClosestExitPointDistance = ce3.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance;

                        ClosestMajorArea final1 = cma1.OrderBy(x => x.ClosestExitPointDistance).Take(1).FirstOrDefault();

                        ma1 = _db.MajorAreas.Where(x => x.AreaName == final1.AreaName).FirstOrDefault();

                        double offroute1 = 0;

                        if (final1.ClosestExitPointDistance > 6 & final1.ClosestExitPointDistance <= 50)
                        { offroute1 = 50; }
                        else if (final1.ClosestExitPointDistance > 50 & final1.ClosestExitPointDistance <= 100)
                        { offroute1 = 100; }
                        else if (final1.ClosestExitPointDistance > 100)
                        { offroute1 = 150; }


                        //// GET MAJOR AREA 2 CLOSEST EXIT POINTS ////
                        for (int i = 0; i < cma2.Count(); i++)
                        {
                            MajorAreaRouteExit exit = new MajorAreaRouteExit();

                            exit = GetRouteExits(final1.AreaName, cma2[i].AreaName, 2);
                            mare2.Add(exit);
                        }


                        List<ClosestExit> closestexits2 = new List<ClosestExit>();
                        foreach (MajorAreaRouteExit mae in mare2)
                        {
                            closestexits2.Add(GetClosestExit(mae, endPoint.Latitude, endPoint.Longitude));
                        }

                        for (int i = 0; i < cma2.Count(); i++)
                        {
                            cma2[i].ClosestExitPointDistance = closestexits2[i].Distance;
                        }

                        ClosestMajorArea final2 = (ClosestMajorArea)cma2.OrderBy(x => x.ClosestExitPointDistance).FirstOrDefault();

                        ma2 = _db.MajorAreas.Where(x => x.AreaName == final2.AreaName).FirstOrDefault();

                        double offroute2 = 0;

                        if (final2.ClosestExitPointDistance > 6 & final2.ClosestExitPointDistance <= 50)
                        { offroute1 = 50; }
                        else if (final2.ClosestExitPointDistance > 50 & final2.ClosestExitPointDistance <= 100)
                        { offroute2 = 100; }
                        else if (final2.ClosestExitPointDistance > 100)
                        { offroute2 = 150; }

                        Route baseprice = _db.Routes.Where(y => y.Area1 == ma1.AreaName && y.Area2 == ma2.AreaName).FirstOrDefault();

                        double multiplier = 1 + service.GetVehicleMultiplier(Make, Model);
                        double price = Convert.ToDouble(baseprice.Price) * multiplier + (150) + (offroute1 * multiplier) + (offroute2 * multiplier);

                        q.Vehicle = Make + " " + Model;
                        q.Area1 = address1;
                        q.Area2 = address2;
                        q.Price = (int)Math.Round(price, 1);

                        sb.AppendLine("OffRoute Area 1: Closest Major Areas - " + cma1[0].AreaName + "," + cma1[1].AreaName + "," + cma1[2].AreaName + "<br /> ");
                        sb.AppendLine("Closest Major Area Exit - Interstate: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Interstate + " Junction:" + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Junction + " Distance: " + closestexits.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance + "<br /> ");
                        sb.AppendLine("OffRoute Area 2: Closest Major Areas - " + cma2[0].AreaName + "," + cma2[1].AreaName + "," + cma2[2].AreaName + "<br /> ");
                        sb.AppendLine("Closest Major Area Exit - Interstate: " + closestexits2.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Interstate + " Junction:" + closestexits2.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Junction + " Distance: " + closestexits2.OrderBy(x => x.Distance).Take(1).FirstOrDefault().Distance + "<br /> ");
                        sb.AppendLine("Major Area 1: " + ma1.AreaName + " - " + ma1.MainCityName + "<br /> ");
                        sb.AppendLine("Major Area 2: " + ma2.AreaName + " - " + ma2.MainCityName + "<br /> ");
                        sb.AppendLine("Base Price: " + baseprice.Price + "<br /> ");
                        sb.AppendLine("Vehicle Category Multiplier: " + multiplier + "<br /> ");
                        sb.AppendLine("Broker Fee: 150 <br /> ");
                        sb.AppendLine("OffRoute1 * Multiplier: " + (offroute1 * multiplier) + "<br /> ");
                        sb.AppendLine("OffRoute2 * Multiplier: " + (offroute2 * multiplier) + "<br /> ");
                        sb.AppendLine("Quote: " + baseprice.Price + " * " + multiplier + " + 150(Broker Fee) + " + (offroute1 * multiplier) + " + " + (offroute2 * multiplier) + " = " + q.Price);

                        ViewBag.Log = sb.ToString();

                    }

                }
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

        
        public MajorAreaRouteExit GetRouteExits(string ma1, string ma2, int dest)
        {

            if (Destination == null && dest == 2)
            { Destination = ma2; }
            if (Destination == null && dest == 1)
            { Destination = ma1; }
            if (Destination == null && dest == 0)
            { Destination = ma1; }

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
            
            if (Passthrough == null)
            { isFinal = maExits.Where(f => f.PassThroughMA == Destination).FirstOrDefault(); }
            else
            { isFinal = maExits.Where(f => f.PassThroughMA == Passthrough).FirstOrDefault(); }

            if (isFinal == null)
            {
                MajorAreaRouteExit cont = maExits.FirstOrDefault();
                int Area = Convert.ToInt32(Destination.Replace("A", ""));
                int Pass = Convert.ToInt32(cont.PassThroughMA.Replace("A", ""));

                if (("A" + Pass) == Destination)
                { 
                    isFinal = maExits.Where(f => f.PassThroughMA == Destination).FirstOrDefault();
                    Passthrough = null;
                    Destination = null;
                    return isFinal;
                }
          
                Passthrough = cont.PassThroughMA;

                if (Area < Pass)
                { GetRouteExits(Destination, "A" + Pass.ToString(), dest); }
                else
                { GetRouteExits("A" + Pass.ToString(), Destination, dest); }
         

            }
            Passthrough = null;
            Destination = null;
            return isFinal;

        }

        public List<ClosestMajorArea> GetClosestMA(double oLat, double oLong, MajorArea majorarea)
        {
            List<ClosestMajorArea> list = new List<ClosestMajorArea>();

            var oCoord = new GeoCoordinate(oLat, oLong);

            var ma = _db.MajorAreas;

            if (majorarea != null)
            {

                foreach (MajorArea m in ma)
                {
                    try
                    {
                        ZipcodePoolsPivotPoint z = _db.ZipcodePoolsPivotPoints.Where(x => x.PoolName == m.Pool1Name && x.State == m.Pool1State).FirstOrDefault();

                        ClosestMajorArea cma = new ClosestMajorArea();
                        cma.AreaName = m.AreaName;
                        cma.ZipCode = z.ZipCode;
                        cma.Latitude = z.Latitude;
                        cma.Longitude = z.Longitude;

                        var maCoord = new GeoCoordinate(cma.Latitude, cma.Longitude);

                        cma.Distance = (oCoord.GetDistanceTo(maCoord) * 3.28084) / 5280;

                        list.Add(cma);
                    }
                    catch { }
                }

                list = list.Where(x => x.AreaName != majorarea.AreaName).ToList();
                list = list.OrderBy(x => x.Distance).Take(3).ToList();
            }



            return list;
        }

        public List<ClosestMajorArea> GetClosestMA(double oLat, double oLong, List<ClosestMajorArea> majorarea)
        {
            List<ClosestMajorArea> list = new List<ClosestMajorArea>();

            var oCoord = new GeoCoordinate(oLat, oLong);

            var ma = _db.MajorAreas;

            if (majorarea != null)
            {

                foreach (MajorArea m in ma)
                {
                    try
                    {
                        ZipcodePoolsPivotPoint z = _db.ZipcodePoolsPivotPoints.Where(x => x.PoolName == m.Pool1Name && x.State == m.Pool1State).FirstOrDefault();

                        ClosestMajorArea cma = new ClosestMajorArea();
                        cma.AreaName = m.AreaName;
                        cma.ZipCode = z.ZipCode;
                        cma.Latitude = z.Latitude;
                        cma.Longitude = z.Longitude;

                        var maCoord = new GeoCoordinate(cma.Latitude, cma.Longitude);

                        cma.Distance = (oCoord.GetDistanceTo(maCoord) * 3.28084) / 5280;

                        list.Add(cma);
                    }
                    catch { }
                }

                list = list.Where(x => x.AreaName != majorarea[0].AreaName && x.AreaName != majorarea[1].AreaName && x.AreaName != majorarea[2].AreaName).ToList();
                list = list.OrderBy(x => x.Distance).Take(3).ToList();
            }



            return list;
        }

        public ClosestExit GetClosestExit(MajorAreaRouteExit ma, double lat, double lon)
        {
            var routeexits = _db.MajorAreaRouteExits.Where(x => x.start == ma.start && x.finish == ma.finish);
            
            List<ClosestExit> list = new List<ClosestExit>();


            foreach (MajorAreaRouteExit m in routeexits)
            {
                int eFrom = Convert.ToInt32(m.Exit_from);
                int eTo = Convert.ToInt32(m.Exit_to);

                if ((eFrom == 0 && eTo == 0) && ma.interstate == null)
                {
                    ClosestExit exit = new ClosestExit();

                    exit.AreaName = null;
                    exit.Interstate = null;
                    exit.Junction = null;
                    exit.Latitude = 0;
                    exit.Longitude = 0;

                    //var eCoord = new GeoCoordinate(exit.Latitude, exit.Longitude);

                    exit.Distance = 1000;


                    list.Add(exit);
                }
                else if ((eFrom == 0 && eTo == 0) && ma.interstate != null)
                {
                    var exits = _db.USInterstateExits.Where(x => x.STATE == m.state && x.HIGHWAY_ID == m.interstate);

                    var oCoord = new GeoCoordinate(lat, lon);

                    foreach (USInterstateExit ex in exits)
                    {
                        
                        ClosestExit exit = new ClosestExit();

                        exit.AreaName = ma.start;
                        exit.Interstate = m.interstate;
                        exit.Junction = ex.Junction_ID;
                        exit.Latitude = ex.LATITUDE;
                        exit.Longitude = ex.LONGITUDE;

                        var eCoord = new GeoCoordinate(exit.Latitude, exit.Longitude);

                        exit.Distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;


                        list.Add(exit);

                    }

                }
                else
                {

                    var exits = _db.USInterstateExits.Where(x => x.STATE == m.state && x.HIGHWAY_ID == m.interstate);

                    var oCoord = new GeoCoordinate(lat, lon);

                    foreach (USInterstateExit ex in exits)
                    {
                        if (Convert.ToInt32(ex.Junction_ID) >= eFrom && Convert.ToInt32(ex.Junction_ID) <= eTo)
                        {
                            ClosestExit exit = new ClosestExit();

                            exit.AreaName = ma.start;
                            exit.Interstate = m.interstate;
                            exit.Junction = ex.Junction_ID;
                            exit.Latitude = ex.LATITUDE;
                            exit.Longitude = ex.LONGITUDE;

                            var eCoord = new GeoCoordinate(exit.Latitude, exit.Longitude);

                            exit.Distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;


                            list.Add(exit);
                        }

                    }
                }
            }

            var last = list.OrderBy(x => x.Distance).Take(1);

            ClosestExit final = last.FirstOrDefault();

            return final;

            
        }

        public double GetZiptoZipDistance(double lat1, double lon1, double lat2, double lon2)
        {

            var oCoord = new GeoCoordinate(lat1, lon1);
            var eCoord = new GeoCoordinate(lat2, lon2);

            double Distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;

            return Distance;
        }

        public bool isAddressZip(String address)
        {
            return Regex.IsMatch(address, @"^\d{5}$");
        }

        public USZipcode GetUSZipcode(string address)
        {
            USZipcode zipcode = new USZipcode();

            //Check if address is Zip Code
            if (isAddressZip(address))
            {
                zipcode = _db.USZipcodes.Where(x => x.ZIPCode == address).FirstOrDefault();
                
            }
            else
            {
                char[] delimiterChars = { ',' };
                string[] add = address.Split(delimiterChars);

                string City = add[0].ToString();
                string State = add[1].Trim();

                zipcode = _db.USZipcodes.Where(x => x.CityName == City && (x.StateAbbr == State || x.StateName == State)).FirstOrDefault();

            }

            return zipcode;
        }

        public double GetDistanceToMAover30(USZipcode point, MajorArea ma)
        {
            double distance = 0;

            MajorAreaCoord maCoords = _db.MajorAreaCoords.Where(x => x.MajorArea == ma.AreaName).FirstOrDefault();

            var oCoord = new GeoCoordinate(point.Latitude, point.Longitude);
            var eCoord = new GeoCoordinate(maCoords.Latitude, maCoords.Longitude);

            distance = (oCoord.GetDistanceTo(eCoord) * 3.28084) / 5280;
            

            return distance;
        }
        
        
        
        public void TestZips()
        {
            List<ZipcodePool> zips1 = _db.ZipcodePools.ToList();
            List<ZipcodePool> zips2 = _db.ZipcodePools.ToList();

            //foreach (ZipcodePool zp1 in zips1)
            //{
                //string z1 = zp1.ZipCode;
                string z1 = "66523";

                foreach (ZipcodePool zp2 in zips2)
                {
                    string z2 = zp2.ZipCode;
                    try
                    {
                        if (z1 != z2)
                        {
                            GetPrice(z1, z2, "Acura | CL");
                        }
                    }
                    catch (Exception ex)
                    {
                        //System.IO.File.WriteAllText(@"C:\Projects\EagleTrucking\logs\" + z1 + "-" + z2 + ".txt", zp1.City + " " + zp1.State + " - " + zp2.City + " " + zp2.State + "\r\n" +  ex.ToString());
                        System.IO.File.WriteAllText(@"C:\Projects\EagleTrucking\logs\" + z1 + "-" + z2 + ".txt", "Osage City KS - " + zp2.City + " " + zp2.State + "\r\n" + ex.ToString());
                    }
                }

            //}

        }
        
        
    }
}
