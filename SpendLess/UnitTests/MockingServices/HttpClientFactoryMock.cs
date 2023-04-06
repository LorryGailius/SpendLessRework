namespace SpendLess.UnitTests.MockingServices
{
    internal class HttpClientFactoryMock<T> : IHttpClientFactory
    {
        public HttpClientFactoryMock(T instance)
        {
            _instance = instance;
        }
        private T _instance;
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(new HttpMessageHandlerMock<T>(_instance));
        }
    }
}
