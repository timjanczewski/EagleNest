using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteCalculator.Domain.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Class { get; set; }
        public decimal PricePercentIncrease { get; set; }

    }
}
