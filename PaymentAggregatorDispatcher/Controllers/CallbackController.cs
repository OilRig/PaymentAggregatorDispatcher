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

        private async Task EndRequest(HttpResponseMessage httpResponseMessage)
        {
            if(httpResponseMessage != null)
            {
                CopyFromTargetResponseHeaders(HttpContext, httpResponseMessage);

                await httpResponseMessage.Content.CopyToAsync(HttpContext.Response.Body);
            }
            
            await HttpContext.Response.CompleteAsync();
        }

        [HttpPost]
        [Route("payments/callback/{paymentMethod}/{activity}")]
        public async Task<IActionResult> HandlePayment(string paymentMethod, string activity)
        {
            try
            {
                EPaymentMethod method = (EPaymentMethod)Enum.Parse(typeof(EPaymentMethod), paymentMethod, true);
                ETransactionIntent transactionIntent = GetIntentIfPossibleFromString(activity);

                _logger.LogInformation($"Handling callback for {method} - {transactionIntent}");

                if (transactionIntent == ETransactionIntent.Undefined)
                {
                    _logger.LogError($"Unknown transaction intent {transactionIntent}");

                    return BadRequest();
                }

                AwaitingAddressResult[] tokens = await _paymentDispatcherDomain.GetUniqueTokensForAwaitingAggregators(transactionIntent, method);

                foreach(AwaitingAddressResult address in tokens)
                {
                    using HttpRequestMessage targetRequestMessage = CreateTargetMessage(HttpContext, new Uri($"{address.AggregatorAddress}/callback/{paymentMethod}/{activity}"));

                    using HttpResponseMessage responseMessage = await _httpClientSvc.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, HttpContext.RequestAborted);

                    _logger.LogInformation($"Aggregator with address {address.AggregatorAddress} response status: {responseMessage.StatusCode}");

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;

                        await _paymentDispatcherDomain.SetDispatchRequestCompleted(address.RequestToken);

                        _logger.LogInformation($"Request with token {address.RequestToken} notified to {address.AggregatorAddress}");

                        await EndRequest(responseMessage);

                        break;
                    }
                }

                HttpContext.Response.StatusCode = 500;

                await EndRequest(null);

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
