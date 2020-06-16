using System;
using System.Net.Http;
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
        [RequiredParameter]
        public string WebhookUrl { get; set; }

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            var emoticon = this.GetEmoticon(logEvent.Level);
            var hostName = Environment.MachineName;
            var renderedLog = this.RenderLogEvent(this.Layout, logEvent);

            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine($"{emoticon} [{logEvent.Level.ToString().ToUpper()}] on {hostName}");
            contentBuilder.AppendLine();
            contentBuilder.Append(renderedLog);

            var message = JsonSerializer.Serialize(
                new { Msgtype = "text", Text = new { Content = contentBuilder.Replace("\r", string.Empty).ToString() } },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var webhook = new Uri(this.WebhookUrl);

            var request = new HttpRequestMessage(HttpMethod.Post, webhook)
                          {
                              Content = new StringContent(message, Encoding.UTF8, "application/json")
                          };

            var httpClient = HttpClientFactory.Instance.CreateClient(new Uri($"{webhook.Scheme}{Uri.SchemeDelimiter}{webhook.Host}"));

            await httpClient.SendAsync(request, cancellationToken);
        }

        private string GetEmoticon(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Warn) return "[流汗]";
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Fatal) return "[大哭]";

            return "[广播]";
        }
    }
}