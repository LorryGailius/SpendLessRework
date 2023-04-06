using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using SpendLess.Server.Middleware;

namespace Tests.MockingServices
{
    public class RateLimitingMiddlewareMock : RateLimitingMiddleware
    {
        public ClientStatistics? value;
        public RateLimitingMiddlewareMock(RequestDelegate next, IDistributedCache cache, ClientStatistics? clientStatistics) : base(next, cache)
        {
            value = clientStatistics;
        }
        protected override Task<ClientStatistics?> GetCacheValueAsync(IDistributedCache cache, string key)
        {
            //var task = new Task<ClientStatistics> (() => new ClientStatistics()
            //{
            //     LastSuccessfulResponseTime = DateTime.UtcNow,
            //     NumberOfRequestsCompletedSuccessfully = 10
            //});
            //return await Task.Run(() => new ClientStatistics()
            //{
            //    LastSuccessfulResponseTime = DateTime.UtcNow,
            //    NumberOfRequestsCompletedSuccessfully = 10
            //});
            //return await task;
            //var task = new ClientStatistics()
            //{
            //    LastSuccessfulResponseTime = DateTime.UtcNow,
            //    NumberOfRequestsCompletedSuccessfully = 10
            //};
            return Task.FromResult(value);
        }
        protected override Task SetCahceValueAsync(IDistributedCache cache, string key, ClientStatistics? clientStat)
        {
            value = clientStat;
            return Task.FromResult(value);
        }
    }
}
