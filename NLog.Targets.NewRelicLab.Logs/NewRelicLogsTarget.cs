using Cysharp.Text;
using NewRelic.LogEnrichers.NLog;
using NLog.Common;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.NewRelicLab.Logs
{
    [Target("NewRelicLogs")]
    public sealed class NewRelicLogsTarget : AsyncTaskTarget
    {
        private readonly HttpClient client = new HttpClient();

        private string licenseKey;
        public string LicenseKey 
        {
            set
            {
                licenseKey = value;
                client.DefaultRequestHeaders.Remove("X-License-Key");
                client.DefaultRequestHeaders.Add("X-License-Key", licenseKey);
            }
        }

        private string insertKey;
        public string InsertKey
        { 
            set
            {
                insertKey = value;
                client.DefaultRequestHeaders.Remove("X-License-Key");
                client.DefaultRequestHeaders.Add("X-License-Key", insertKey);
            }
        }

        public string Endpoint { get; set; }

        public NewRelicLogsTarget()
        {
            Layout = new NewRelicJsonLayout();
            Endpoint = "https://log-api.newrelic.com/log/v1";
            BatchSize = 10;
            var licenseKey = Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
            if (!string.IsNullOrEmpty(licenseKey))
            {
                LicenseKey = licenseKey;
                InternalLogger.Debug("X-License-Key is configured by NEW_RELIC_LICENSE_KEY environment variable.");
            }
            else
            {
                var insertKey = Environment.GetEnvironmentVariable("NEW_RELIC_INSERT_KEY");
                if (!string.IsNullOrEmpty(insertKey))
                {
                    InsertKey = insertKey;
                    InternalLogger.Debug("X-Insert-Key is configured by NEW_RELIC_INSERT_KEY environment variable.");
                }
            }
        }

        protected override Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken) => Task.CompletedTask;

        protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            try
            {
                if (!logEvents.Any())
                {
                    InternalLogger.Debug("log event is empty.");
                    return;
                }

                using var sb = ZString.CreateUtf8StringBuilder();
                sb.Append("[{\"logs\": [");
                var delimStart = string.Empty;
                foreach (var logEvent in logEvents)
                {
                    sb.Append(delimStart);
                    sb.Append(Layout.Render(logEvent));
                    delimStart = ",";
                }
                sb.Append("]}]");
                var res = await client.PostAsync(Endpoint, new GZipZStringContent(sb));
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "failed to send.");
            }
        }
    }

    public class GZipZStringContent : HttpContent
    {
        private readonly Utf8ValueStringBuilder sb;

        public GZipZStringContent(Utf8ValueStringBuilder sb)
        {
            Headers.TryAddWithoutValidation("Content-Type", "application/gzip");
            Headers.ContentEncoding.Add("gzip");
            this.sb = sb;

        }
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var gzipStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            return sb.WriteToAsync(gzipStream).ContinueWith(_ =>
            {
                gzipStream?.Dispose();
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1L;
            return false;
        }
    }
}
