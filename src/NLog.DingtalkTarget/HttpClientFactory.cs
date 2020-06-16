using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace NLog.Targets
{
    internal class HttpClientFactory
    {
        private static readonly Lazy<HttpClientFactory> Lazy = new Lazy<HttpClientFactory>(() => new HttpClientFactory());
        private static readonly Dictionary<Uri, HttpClient> HttpClientContainer = new Dictionary<Uri, HttpClient>();

        private HttpClientFactory()
        {
        }

        public static HttpClientFactory Instance => Lazy.Value;

        public HttpClient CreateClient(Uri baseAddress)
        {
            if (HttpClientContainer.ContainsKey(baseAddress)) return HttpClientContainer[baseAddress];

            lock (HttpClientContainer)
            {
                if (HttpClientContainer.ContainsKey(baseAddress)) return HttpClientContainer[baseAddress];

                var client = new HttpClient { BaseAddress = baseAddress };

                ServicePointManager.FindServicePoint(baseAddress).ConnectionLeaseTimeout = 60000;

                HttpClientContainer[baseAddress] = client;

                return client;
            }
        }
    }
}