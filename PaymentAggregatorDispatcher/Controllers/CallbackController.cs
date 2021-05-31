using BaltBet.Payment.Data.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentAggregatorDispatcher.HttpClients;
using PaymentDispatcher.Data.Identities;
using PaymentDispatcher.Database.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.Controllers
{
    public class CallbackController : Controller
    {
        private readonly IPaymentDispatcherDomain _paymentDispatcherDomain;
        private readonly ILogger<CallbackController> _logger;
        private readonly IDispatcherHttpClient _httpClientSvc;

        public CallbackController(IPaymentDispatcherDomain paymentDispatcherDomain,
           ILogger<CallbackController> logger, IDispatcherHttpClient dispatcherHttpClient)
        {
            _paymentDispatcherDomain = paymentDispatcherDomain;
            _logger = logger;
            _httpClientSvc = dispatcherHttpClient;
        }

        struct AwaitingAddressResult
        {
            public AwaitingAddressResult(string address, string requestToken)
            {
                AggregatorAddress = address;
                RequestToken = requestToken;
            }
            public string AggregatorAddress { get; set; }

            public string RequestToken { get; set; }

            public static AwaitingAddressResult Empty => new(string.Empty, string.Empty);
        }

        private async ValueTask<AwaitingAddressResult> GetAwaitingAggregatorAdddress(DispatcherRequest[] requests)
        {
            foreach(DispatcherRequest dispatcherRequest in requests)
            {
                using JsonContent content = JsonContent.Create(dispatcherRequest, typeof(DispatcherRequest));

                string aggregatorAddress = await _paymentDispatcherDomain.GetAggregatorAddressByToken(dispatcherRequest.UniqueAggregatorToken);

                var response = await _httpClientSvc.PostAsync($"{aggregatorAddress}/api/payments/dispatcher/getpayment", content);

                if (response.IsSuccessStatusCode)
                    return new AwaitingAddressResult(aggregatorAddress, dispatcherRequest.UniqueRequestToken);
            }

            return AwaitingAddressResult.Empty;
        }
        private static ETransactionIntent GetIntentIfPossibleFromString(string intentStr)
        {
            if(Enum.TryParse(typeof(ETransactionIntent), intentStr, true, out var normalIntent))
            {
                return (ETransactionIntent)normalIntent;
            }
            else if(Enum.TryParse(typeof(Models.MapEnums.EPluralIntent), intentStr, true, out var pluralIntent))
            {
                Models.MapEnums.EPluralIntent ePlural = (Models.MapEnums.EPluralIntent)pluralIntent;

                switch(ePlural)
                {
                    case Models.MapEnums.EPluralIntent.Invoices:
                        return ETransactionIntent.Invoice;
                    case Models.MapEnums.EPluralIntent.Payouts:
                        return ETransactionIntent.Payout;
                    default: return ETransactionIntent.Undefined;
                }
            }
            else if(Enum.TryParse(typeof(Models.MapEnums.ENotificationIntent), intentStr, true, out var notificationIntent))
            {
                Models.MapEnums.ENotificationIntent notifyIntent = (Models.MapEnums.ENotificationIntent)notificationIntent;

                switch (notifyIntent)
                {
                    case Models.MapEnums.ENotificationIntent.NotificationInvoice:
                        return ETransactionIntent.Invoice;
                    case Models.MapEnums.ENotificationIntent.NotificationPayout:
                        return ETransactionIntent.Payout;
                    default: return ETransactionIntent.Undefined;
                }
            }

            return ETransactionIntent.Undefined;
        }
        private static HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();

            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }
        private static void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);

                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
        private static void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }


        [HttpPost]
        [Route("payments/callback/{paymentMethod}/{activity}")]
        public async Task<IActionResult> HandlePayment(string paymentMethod, string activity)
        {
            try
            {
                EPaymentMethod method = (EPaymentMethod)Enum.Parse(typeof(EPaymentMethod), paymentMethod, true);
                ETransactionIntent transactionIntent = GetIntentIfPossibleFromString(activity);

                if (transactionIntent == ETransactionIntent.Undefined)
                    return BadRequest();

                DispatcherRequest[] tokens = await _paymentDispatcherDomain.GetUniqueTokensForAwaitingAggregators(transactionIntent, method);

                AwaitingAddressResult aggregatorAddress = await GetAwaitingAggregatorAdddress(tokens);

                using HttpRequestMessage targetRequestMessage = CreateTargetMessage(HttpContext, new Uri($"{aggregatorAddress}/callback/{paymentMethod}/{activity}"));

                using HttpResponseMessage responseMessage = await _httpClientSvc.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

                HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(HttpContext, responseMessage);
                await responseMessage.Content.CopyToAsync(HttpContext.Response.Body);

                if(responseMessage.IsSuccessStatusCode)
                {
                    await _paymentDispatcherDomain.SetDispatchRequestCompleted(aggregatorAddress.RequestToken);
                }    

                await HttpContext.Response.CompleteAsync();

                return null;
            }
            catch (Exception ex)
            {

                _logger.LogError($"Error Handling Payment: {ex.Message}");

                return NotFound();
            }
        }
    }
}
