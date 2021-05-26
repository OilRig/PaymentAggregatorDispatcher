using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.Models
{
    public class MapModel
    {
        public string Token { get; set; }
        public string Address { get; set; }
    }
    public class SetupModel
    {
        public MapModel[] Maps { get; set; }
    }
}
