using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.Models.MapEnums
{
    public enum EPluralIntent
    {
        Invoices = 10,
        Payouts = 20
    }

    public enum ENotificationIntent
    {
        NotificationInvoice = 10,
        NotificationPayout = 20
    }
}
