using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class MajorArea
    {
        [Key]
        public string MainCityName { get; set; }
        public string AreaName { get; set; }
        public string Pool1State { get; set; }
        public string Pool1Name { get; set; }
        public string Pool2State { get; set; }
        public string Pool2Name { get; set; }
        public string Pool3State { get; set; }
        public string Pool3Name { get; set; }
        public string Pool4State { get; set; }
        public string Pool4Name { get; set; }
    }
}
