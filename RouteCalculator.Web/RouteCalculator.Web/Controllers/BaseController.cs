using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RouteCalculator.Domain.Entities;

namespace RouteCalculator.Web.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/
        protected RouteDbContext _db;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Uri url = Request.Url;
            string protocol = url.Scheme;

            _db = new RouteDbContext();

            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            _db.SaveChanges();
            _db.Dispose();
            base.OnActionExecuted(filterContext);
        }


    }
}
