using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class RouteRhumb
    {
        [Key]
        public string Zipcode1 { get; set; }
        public double lat1 { get; set; }
        public double lon1 { get; set; }
        public string ZipCode2 { get; set; }
        public double lat2 { get; set; }
        public double lon2 { get; set; }
        public double Rhumb { get; set; }
        public double Distance { get; set; }
    }
}
