using Newtonsoft.Json;
using System.Text;

namespace SpendLess.UnitTests.MockingServices
{
    internal class HttpMessageHandlerMock<T> : HttpMessageHandler
    {
        StringContent istringContent;
        public HttpMessageHandlerMock(T instance) 
        {
            string json = JsonConvert.SerializeObject(instance);
            istringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
        }
        

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = istringContent
            });
        }

        
    }
}