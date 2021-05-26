using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaymentDispatcher.Database.Helpers
{
    public static class CheckSumHelper
    {
        public static string GetRequestUniqueString(PaymentDispatcher.Data.Identities.DispatcherRequest request, int randomInt)
        {
            int numericKey = ((byte)request.Intent ^ (byte)request.Method) * randomInt;

            StringBuilder stringBuilder = new();

            stringBuilder.Append(numericKey)
                .Append(request.UniqueAggregatorToken)
                .Append(request.UniqueRequestToken);

            return stringBuilder.ToString();
        }

        public static bool CheckRequestCheckSum(string dbhash, PaymentDispatcher.Data.Identities.DispatcherRequest request, int randomInt)
        {
            string currentSum = GetRequestUniqueString(request, randomInt);

            return dbhash == currentSum;
        }
    }
}
