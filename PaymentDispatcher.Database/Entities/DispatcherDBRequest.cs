using BaltBet.Payment.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Database.Entities
{
    public class DispatcherDBRequest
    {
        public long Id { get; set; }

        public string UniqueAggregatorToken { get; set; }

        public ETransactionIntent Intent { get; set; }

        public EPaymentMethod Method { get; set; }

        public DateTime CreateDate { get; set; }

        public bool IsActive { get; set; }

        public string UniqueRequestToken { get; set; }

        public decimal Amount { get; set; }

        public EPaymentChannel Channel { get; set; }
    }
}
