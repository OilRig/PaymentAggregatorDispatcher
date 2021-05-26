using PaymentDispatcher.Data.Identities;
using PaymentDispatcher.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BaltBet.Payment.Data.Enums;

namespace PaymentDispatcher.Database.Domain
{
    public interface IPaymentDispatcherDomain
    {
        Task AddDispatcherRecord(DispatcherDBRequest dispatcherDBRequest);

        ValueTask<DispatcherRequest[]> GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method);

        Task SetDispatchRequestCompleted(string dispatcherRequestToken);

        ValueTask<string> GetAggregatorAddressByToken(string token);

        ValueTask<DTO.AggregatorTokenMap[]> GetMaps();
    }

    public class PaymentDispatcherDomain : IPaymentDispatcherDomain
    {
        async Task IPaymentDispatcherDomain.AddDispatcherRecord(DispatcherDBRequest dispatcherDBRequest)
        {
            using Context.DatabaseContext ctx = new();

            await ctx.DispatcherPaymentRequests.AddAsync(dispatcherDBRequest);

            await ctx.SaveChangesAsync();
        }

        async Task IPaymentDispatcherDomain.SetDispatchRequestCompleted(string dispatcherRequestToken)
        {
            using Context.DatabaseContext ctx = new();

            var request = await ctx.DispatcherPaymentRequests.FirstOrDefaultAsync(req => req.UniqueRequestToken == dispatcherRequestToken
            && req.IsActive);

            if (request != null)
            {
                request.IsActive = false;

                ctx.Entry(request).State = EntityState.Modified;

                await ctx.SaveChangesAsync();
            }
        }

        async ValueTask<string> IPaymentDispatcherDomain.GetAggregatorAddressByToken(string token)
        {
            using Context.DatabaseContext ctx = new();

            var map = await ctx.AggregatorTokenMaps.FirstOrDefaultAsync(map => map.AggregatorToken == token);

            if (map != null)
                return map.AggregatorAddress;

            return string.Empty;
        }

        async ValueTask<DTO.AggregatorTokenMap[]> IPaymentDispatcherDomain.GetMaps()
        {
            using Context.DatabaseContext ctx = new();
            return await ctx.AggregatorTokenMaps.Select(m => new DTO.AggregatorTokenMap()
            {
                Address = m.AggregatorAddress,
                Token   = m.AggregatorToken,
                Id      = m.Id
            }).ToArrayAsync();
        }

        async ValueTask<DispatcherRequest[]> IPaymentDispatcherDomain.GetUniqueTokensForAwaitingAggregators(ETransactionIntent intent, EPaymentMethod method)
        {
            using Context.DatabaseContext ctx = new();

            var requests = await ctx.DispatcherPaymentRequests
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
    }
}
