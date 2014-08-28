using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class USInterstateExit
    {
        [Key]
        public int Id { get; set; }
        public string STATE { get; set; }
        public string HIGHWAY_ID { get; set; }
        public int Junction_ID { get; set; }
        public double LATITUDE { get; set; }
        public double LONGITUDE { get; set; }

    }
}
