using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Database.Entities
{
    public class AggregatorTokenMap
    {
        public int Id { get; set; }

        public string AggregatorToken { get; set; }

        public string AggregatorAddress { get; set; }
    }
}
