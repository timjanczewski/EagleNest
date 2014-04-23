using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class Route
    {
        [Key]
        public string Area1 { get; set; }
        public string Area2 { get; set; }
        public string Price { get; set; }
    }
}
