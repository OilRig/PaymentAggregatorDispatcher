using PaymentDispatcher.Data.Identities;
using PaymentDispatcher.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BaltBet.Payment.Data.Enums;
using PaymentDispatcher.Database.Context;

namespace PaymentDispatcher.Database.Domain
{
    public interface IPaymentDispatcherDomain
    {
        Task AddDispatcherRecord(DispatcherDBRequest dispatcherDBRequest);

        Task CancelDispatcherRequest(DispatcherRequest dispatcherRequest);
        ValueTask<AwaitingAddressResult[]> GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method);

        Task SetDispatchRequestCompleted(string dispatcherRequestToken);

        ValueTask<DTO.AggregatorTokenMap[]> GetMaps();
    }

    public struct AwaitingAddressResult
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

    public class PaymentDispatcherDomain : IPaymentDispatcherDomain
    {
        private readonly DatabaseContext _context;

        public PaymentDispatcherDomain(DatabaseContext context)
        {
            _context = context;
        }

        async Task IPaymentDispatcherDomain.AddDispatcherRecord(DispatcherDBRequest dispatcherDBRequest)
        {
            await _context.DispatcherPaymentRequests.AddAsync(dispatcherDBRequest);

            await _context.SaveChangesAsync();
        }

        async Task IPaymentDispatcherDomain.SetDispatchRequestCompleted(string dispatcherRequestToken)
        {
            var request = await _context.DispatcherPaymentRequests.FirstOrDefaultAsync(req => req.UniqueRequestToken == dispatcherRequestToken
            && req.IsActive);

            if (request != null)
            {
                request.IsActive = false;

                _context.Entry(request).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
        }

        async ValueTask<DTO.AggregatorTokenMap[]> IPaymentDispatcherDomain.GetMaps()
        {
            return await _context.AggregatorTokenMaps.Select(m => new DTO.AggregatorTokenMap()
            {
                Address = m.AggregatorAddress,
                Token   = m.AggregatorToken,
                Id      = m.Id
            }).ToArrayAsync();
        }

        async ValueTask<AwaitingAddressResult[]> IPaymentDispatcherDomain.GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method)
        {
            var requests = await _context.DispatcherPaymentRequests
                .Where(r => r.Method == method && r.Intent == intent && r.IsActive)
                .Select(req => new { AggregatorToken = req.UniqueAggregatorToken, RequestToken = req.UniqueRequestToken })
                .ToArrayAsync();

            if(requests.Any())
            {
                var maps = await _context.AggregatorTokenMaps
                    .Where(map => requests.Any(r => r.AggregatorToken == map.AggregatorToken))
                    .ToArrayAsync();

                if(maps.Any())
                {
                    List<AwaitingAddressResult> result = new();

                    foreach(var map in maps)
                    {
                        string requestToken = requests.FirstOrDefault(r => r.AggregatorToken == map.AggregatorToken)?.RequestToken;

                        if (!string.IsNullOrEmpty(requestToken))
                            result.Add(new AwaitingAddressResult(map.AggregatorAddress, requestToken));
                    }

                    return result.ToArray();
                }
            }

            return Array.Empty<AwaitingAddressResult>();
        }

        async Task IPaymentDispatcherDomain.CancelDispatcherRequest(DispatcherRequest dispatcherRequest)
        {
            var request = _context.DispatcherPaymentRequests.FirstOrDefault(r => r.IsActive && r.UniqueRequestToken == dispatcherRequest.UniqueRequestToken
             && r.UniqueAggregatorToken == dispatcherRequest.UniqueAggregatorToken);

            if(request != null)
            {
                request.IsActive = false;

                _context.Entry(request).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
        }
    }
}
