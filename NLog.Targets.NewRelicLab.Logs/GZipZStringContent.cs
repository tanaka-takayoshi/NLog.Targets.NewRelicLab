using Cysharp.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NLog.Targets.NewRelicLab.Logs
{
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
