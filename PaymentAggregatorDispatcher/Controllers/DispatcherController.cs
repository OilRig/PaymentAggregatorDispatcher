using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentDispatcher.Data.Identities;
using PaymentDispatcher.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.Controllers
{
    public class DispatcherController : Controller
    {
        private readonly IPaymentDispatcherDomain _paymentDispatcherDomain;
        private readonly ILogger<DispatcherController> _logger;
        public DispatcherController(IPaymentDispatcherDomain paymentDispatcherDomain,
            ILogger<DispatcherController> logger)
        {
            _paymentDispatcherDomain = paymentDispatcherDomain;
            _logger = logger;
        }

        [HttpPost]
        [Route("payments/dispatcher/request/add")]
        public async Task<IActionResult> AddRequest([FromBody]DispatcherRequest dispatcherRequest)
        {
            try
            {
                await _paymentDispatcherDomain.AddDispatcherRecord(new PaymentDispatcher.Database.Entities.DispatcherDBRequest()
                {
                    CreateDate            = DateTime.Now,
                    IsActive              = true,
                    UniqueAggregatorToken = dispatcherRequest.UniqueAggregatorToken,
                    Intent                = dispatcherRequest.Intent,
                    Method                = dispatcherRequest.Method,
                    UniqueRequestToken    = dispatcherRequest.UniqueRequestToken,
                    Amount                = dispatcherRequest.Amount,
                    Channel               = dispatcherRequest.Channel
                });

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing dispatch request: {ex.Message}");

                return BadRequest();
            }
        }

        [HttpPost]
        [Route("payments/dispatcher/request/cancel")]
        public async Task<IActionResult> DeclineRequest([FromBody] DispatcherRequest dispatcherRequest)
        {
            try
            {
                //await _paymentDispatcherDomain.CancelDispatchRequest(dispatcherRequest);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cancelling dispatch request: {ex.Message}");

                return BadRequest();
            }
        }
    }
}
