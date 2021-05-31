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
        ValueTask<DispatcherRequest[]> GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method);

        Task SetDispatchRequestCompleted(string dispatcherRequestToken);

        ValueTask<string> GetAggregatorAddressByToken(string token);

        ValueTask<DTO.AggregatorTokenMap[]> GetMaps();
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

        async ValueTask<string> IPaymentDispatcherDomain.GetAggregatorAddressByToken(string token)
        {
            var map = await _context.AggregatorTokenMaps.FirstOrDefaultAsync(map => map.AggregatorToken == token);

            if (map != null)
                return map.AggregatorAddress;

            return string.Empty;
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

        async ValueTask<DispatcherRequest[]> IPaymentDispatcherDomain.GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method)
        {
            var requests = await _context.DispatcherPaymentRequests
                .Where(r => r.Method == method && r.Intent == intent && r.IsActive)
                .ToArrayAsync();

            if(requests.Any())
            {
                return requests.Select(req => new DispatcherRequest()
                {
                    Amount                = req.Amount,
                    Channel               = req.Channel,
                    Intent                = req.Intent,
                    Method                = req.Method,
                    UniqueAggregatorToken = req.UniqueAggregatorToken,
                    UniqueRequestToken    = req.UniqueRequestToken
                }).ToArray();
            }

            return Array.Empty<DispatcherRequest>();
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
