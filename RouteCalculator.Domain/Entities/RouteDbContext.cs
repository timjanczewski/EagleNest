using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace RouteCalculator.Domain.Entities
{
    public class RouteDbContext : DbContext
    {
        public RouteDbContext()
        {
        }
        public RouteDbContext(string conectionString)
            : base(conectionString)
        {

        }

        //Tables Go Here
        public DbSet<Route> Routes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<MajorArea> MajorAreas { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ZipcodePool> ZipcodePools { get; set; }
        public DbSet<ZipcodePoolsPivotPoint> ZipcodePoolsPivotPoints { get; set; }
        
    }
}
