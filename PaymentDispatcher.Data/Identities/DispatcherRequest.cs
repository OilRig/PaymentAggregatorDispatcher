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
        public string UniqueToken { get; set; }
        public EPaymentMethod Method { get; set; }

        public override int GetHashCode()
        {
            return new {Intent, UniqueToken, Method }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is DispatcherRequest request)
            {
                return request.Intent == this.Intent
                    && request.Method == this.Method
                    && request.UniqueToken == this.UniqueToken;
            }

            return false;
        }
    }
}
