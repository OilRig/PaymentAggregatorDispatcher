using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Database.DTO
{
    public class AggregatorTokenMap
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string Address { get; set; }
    }
}
