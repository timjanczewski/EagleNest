using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class MajorAreaRouteExit
    {
        [Key]
        public int Id { get; set; }
        public string start { get; set; }
        public string finish { get; set; }
        public int RouteNumber { get; set; }
        public string RouteLevel { get; set; }
        public string state { get; set; }
        public string interstate { get; set; }
        public string Exit_from { get; set; }
        public string Exit_to { get; set; }
        public string PassThroughMA { get; set; }

    }
}
