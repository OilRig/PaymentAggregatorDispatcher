using Microsoft.AspNetCore.Mvc;
using PaymentAggregatorDispatcher.Models;
using PaymentDispatcher.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentAggregatorDispatcher.Controllers
{
    public class DispatcherSetupController : Controller
    {
        private readonly IPaymentDispatcherDomain _paymentDispatcherDomain;

        public DispatcherSetupController(IPaymentDispatcherDomain paymentDispatcherDomain)
        {
            _paymentDispatcherDomain = paymentDispatcherDomain;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maps = await _paymentDispatcherDomain.GetMaps();

            return View(new SetupModel()
            {
                Maps = maps.Select(map => new MapModel()
                {
                    Address = map.Address,
                    Token = map.Token
                }).ToArray()
            });
        }
    }
}
