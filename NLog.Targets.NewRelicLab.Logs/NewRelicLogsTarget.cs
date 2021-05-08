using Cysharp.Text;
using NewRelic.LogEnrichers.NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.NewRelicLab.Logs
{
    [Target("NewRelicLogs")]
    public sealed class NewRelicLogsTarget : AsyncTaskTarget
    {
        private readonly HttpClient client = new HttpClient();

        public string LicenseKey { get; set; }

        public string InsertKey { get; set; }

        public string Endpoint { get; set; }

        [DefaultValue(false)]
        public bool EscapeJson { get; set; }

        public NewRelicLogsTarget()
        {
            Layout = new NewRelicJsonLayout();
            Endpoint = "https://log-api.newrelic.com/log/v1";
        }

        protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
            => InternalAsyncTask(logEvent, cancellationToken);

        private async Task InternalAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            InternalLogger.Debug("Sending one log event");
            using var sb = ZString.CreateUtf8StringBuilder();
            sb.Append("[{\"logs\": [");
            if (EscapeJson)
            {
                sb.Append("{\"timestamp\": ");
                sb.Append(new DateTimeOffset(logEvent.TimeStamp).ToUnixTimeMilliseconds());
                sb.Append(",\"message\": \"");
                sb.Append(JsonEncodedText.Encode(Layout.Render(logEvent)));
                sb.Append("\"}");
            }
            else
            {
                sb.Append(Layout.Render(logEvent));
            }
            sb.Append("]}]");

            await SendLogStringBuilder(sb, cancellationToken).ConfigureAwait(false);
        }

        protected override Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
            => InternalAsyncTask(logEvents, cancellationToken);
        
        private async Task InternalAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            if (logEvents.Count == 0)
            {
                InternalLogger.Debug("log event is empty.");
                return;
            }

            InternalLogger.Debug("# of Sending log events: {0}", logEvents.Count);
            using var sb = ZString.CreateUtf8StringBuilder();
            sb.Append("[{\"logs\": [");
            var delimStart = string.Empty;
            foreach (var logEvent in logEvents)
            {
                sb.Append(delimStart);
                if (EscapeJson)
                {
                    sb.Append("{\"timestamp\": ");
                    sb.Append(new DateTimeOffset(logEvent.TimeStamp).ToUnixTimeMilliseconds());
                    sb.Append(",\"message\": \"");
                    sb.Append(JsonEncodedText.Encode(Layout.Render(logEvent)));
                    sb.Append("\"}");
                }
                else
                {
                    sb.Append(Layout.Render(logEvent));
                }

                delimStart = ",";
            }
            sb.Append("]}]");

            await SendLogStringBuilder(sb, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendLogStringBuilder(Utf8ValueStringBuilder sb, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new GZipZStringContent(sb)
            };

            if (!string.IsNullOrEmpty(LicenseKey))
            {
                InternalLogger.Debug("Using license key specified with configuration");
                requestMessage.Headers.Add("X-License-Key", LicenseKey);
            }
            else if (!string.IsNullOrEmpty(InsertKey))
            {
                InternalLogger.Debug("Using insert key specified with configuration");
                requestMessage.Headers.Add("X-Insert-Key", InsertKey);
            }
            else
            {
                var lkey = Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
                if (!string.IsNullOrEmpty(lkey))
                {
                    InternalLogger.Debug("Using licenseKey key specified with NEW_RELIC_LICENSE_KEY environment variable.");
                    requestMessage.Headers.Add("X-License-Key", lkey);
                }
                else
                {
                    var iKey = Environment.GetEnvironmentVariable("NEW_RELIC_INSERT_KEY");
                    if (!string.IsNullOrEmpty(iKey))
                    {
                        InternalLogger.Debug("Using insert key specified with NEW_RELIC_INSERT_KEY environment variable.");
                        requestMessage.Headers.Add("X-Insert-Key", iKey);
                    }
                    else
                    {
                        InternalLogger.Warn("No key found. Abort sending logs.");
                        return;
                    }
                }
            }

            if (InternalLogger.IsTraceEnabled)
                InternalLogger.Trace("Sending content: {0}", sb.ToString());
            var res = await client.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);

            if (res.IsSuccessStatusCode)
            {
                if (InternalLogger.IsTraceEnabled)
                    InternalLogger.Trace("HTTP POST succeeded: {0}", await res.Content.ReadAsStringAsync().ConfigureAwait(false));
                return;
            }

            InternalLogger.Error("Send failed: {0} - {1}", res.StatusCode, await res.Content.ReadAsStringAsync().ConfigureAwait(false));
            throw new Exception($"HTTP POST failed with {res.StatusCode}.");
        }
    }
}
