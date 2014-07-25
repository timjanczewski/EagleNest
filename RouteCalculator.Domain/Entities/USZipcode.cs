using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class USZipcode
    {
        [Key]
        public int ID { get; set; }
        public string ZIPCode { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string StateAbbr { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        

    }
}
