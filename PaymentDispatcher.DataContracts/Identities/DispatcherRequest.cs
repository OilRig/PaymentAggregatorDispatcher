using BaltBet.Payment.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Data.Identities
{
    public class DispatcherRequest 
    {
        public ETransactionIntent Intent { get; set; }
        public string UniqueAggregatorToken { get; set; }
        public EPaymentMethod Method { get; set; }
        public string UniqueRequestToken { get; set; }
        public decimal Amount { get; set; }
        public EPaymentChannel Channel { get; set; }
    }
}
