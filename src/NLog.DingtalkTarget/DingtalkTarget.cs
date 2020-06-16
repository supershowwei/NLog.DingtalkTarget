using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NLog.Config;

namespace NLog.Targets
{
    [Target("Dingtalk")]
    public class DingtalkTarget : AsyncTaskTarget
    {
        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [RequiredParameter]
        public string WebhookUrl { get; set; }

        public string Secret { get; set; }

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            var emoticon = GetEmoticon(logEvent.Level);
            var hostName = Environment.MachineName;
            var renderedLog = this.RenderLogEvent(this.Layout, logEvent);

            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine($"{emoticon} [{logEvent.Level.ToString().ToUpper()}] on {hostName}");
            contentBuilder.AppendLine();
            contentBuilder.Append(renderedLog);

            var message = JsonSerializer.Serialize(
                new { Msgtype = "text", Text = new { Content = contentBuilder.Replace("\r", string.Empty).ToString() } },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var signature = string.IsNullOrEmpty(this.Secret) ? string.Empty : GenerateSignature(this.Secret);

            var webhook = new Uri(string.Concat(this.WebhookUrl, signature));

            var request = new HttpRequestMessage(HttpMethod.Post, webhook)
                          {
                              Content = new StringContent(message, Encoding.UTF8, "application/json")
                          };

            var httpClient = HttpClientFactory.Instance.CreateClient(new Uri($"{webhook.Scheme}{Uri.SchemeDelimiter}{webhook.Host}"));

            await httpClient.SendAsync(request, cancellationToken);
        }

        private static string GetEmoticon(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Warn) return "[流汗]";
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Fatal) return "[大哭]";

            return "[广播]";
        }

        private static string GenerateSignature(string secret)
        {
            var signatureBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(secret))
            {
                var timestamp = (long)DateTime.UtcNow.Subtract(Jan1st1970).TotalMilliseconds;

                signatureBuilder.AppendFormat("&timestamp={0}", timestamp);

                using (var hmacSHA256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    var signature = string.Concat(timestamp, "\n", secret);

                    signature = Convert.ToBase64String(hmacSHA256.ComputeHash(Encoding.UTF8.GetBytes(signature)));

                    signatureBuilder.AppendFormat("&sign={0}", Uri.EscapeDataString(signature));
                }
            }

            return signatureBuilder.ToString();
        }
    }
}