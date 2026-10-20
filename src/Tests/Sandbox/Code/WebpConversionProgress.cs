namespace Sandbox.Code
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>Counters of <see cref="ConvertThumbnailsToWebpSandbox"/>, updated from the worker threads.</summary>
    public class WebpConversionProgress
    {
        private const int RecentErrorsCount = 5;

        private readonly Stopwatch elapsed = Stopwatch.StartNew();

        private readonly Stopwatch converting = new Stopwatch();

        private readonly ConcurrentQueue<string> recentErrors = new ConcurrentQueue<string>();

        private readonly ConcurrentQueue<string> failures = new ConcurrentQueue<string>();

        private long listed;

        private long converted;

        private long failed;

        private long deleted;

        private long pngBytes;

        private long webpBytes;

        public WebpConversionPhase Phase { get; private set; } = WebpConversionPhase.Listing;

        public string Error { get; private set; }

        public long Listed => Interlocked.Read(ref this.listed);

        /// <summary>Gets the number of PNG thumbnails to convert in this run.</summary>
        public long Total { get; private set; }

        /// <summary>Gets the number of PNG thumbnails that already had their WebP.</summary>
        public long AlreadyWebp { get; private set; }

        public long PngsToDelete { get; private set; }

        public long Converted => Interlocked.Read(ref this.converted);

        public long Failed => Interlocked.Read(ref this.failed);

        public long Deleted => Interlocked.Read(ref this.deleted);

        public long PngBytes => Interlocked.Read(ref this.pngBytes);

        public long WebpBytes => Interlocked.Read(ref this.webpBytes);

        public ConvertedThumbnail Last { get; private set; }

        public TimeSpan Elapsed => this.elapsed.Elapsed;

        public double FilesPerSecond =>
            this.converting.Elapsed.TotalSeconds < 1 ? 0 : (this.Converted + this.Failed) / this.converting.Elapsed.TotalSeconds;

        public TimeSpan? Remaining
        {
            get
            {
                var rate = this.FilesPerSecond;
                var left = this.Total - this.Converted - this.Failed;
                return this.Phase == WebpConversionPhase.Converting && rate > 0 ? TimeSpan.FromSeconds(left / rate) : null;
            }
        }

        public IReadOnlyCollection<string> RecentErrors => this.recentErrors.ToArray();

        public IReadOnlyCollection<string> Failures => this.failures.ToArray();

        public void SetListed(long count) => Interlocked.Exchange(ref this.listed, count);

        public void Plan(long total, long alreadyWebp)
        {
            this.Total = total;
            this.AlreadyWebp = alreadyWebp;
        }

        public void StartConverting()
        {
            this.converting.Start();
            this.Phase = WebpConversionPhase.Converting;
        }

        public void StartDeleting(long count)
        {
            this.converting.Stop();
            this.PngsToDelete = count;
            this.Phase = WebpConversionPhase.DeletingPngs;
        }

        public void AddConverted(string pngKey, long pngLength, long webpLength)
        {
            Interlocked.Increment(ref this.converted);
            Interlocked.Add(ref this.pngBytes, pngLength);
            Interlocked.Add(ref this.webpBytes, webpLength);
            this.Last = new ConvertedThumbnail(pngKey, pngLength, webpLength);
        }

        public void AddDeleted() => Interlocked.Increment(ref this.deleted);

        public void AddFailed(string key, string message)
        {
            Interlocked.Increment(ref this.failed);
            var line = $"{key}: {message}";
            this.failures.Enqueue(line);
            this.recentErrors.Enqueue(line);
            while (this.recentErrors.Count > RecentErrorsCount)
            {
                this.recentErrors.TryDequeue(out _);
            }
        }

        public void Finish(WebpConversionPhase phase, string error = null)
        {
            this.converting.Stop();
            this.Error = error;
            this.Phase = phase;
        }

        public record ConvertedThumbnail(string PngKey, long PngBytes, long WebpBytes);
    }
}
