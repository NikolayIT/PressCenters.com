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

        private long processed;

        private long converted;

        private long damaged;

        private long damagedHandled;

        private long fromOriginals;

        private long failed;

        private long deleted;

        private long pngBytes;

        private long webpBytes;

        public WebpConversionPhase Phase { get; private set; } = WebpConversionPhase.Listing;

        public string Error { get; private set; }

        /// <summary>Gets a remark about the run worth reading at its end, if any.</summary>
        public string Note { get; private set; }

        public long Listed => Interlocked.Read(ref this.listed);

        /// <summary>Gets the number of PNG thumbnails to convert in this run.</summary>
        public long Total { get; private set; }

        /// <summary>Gets the number of PNG thumbnails that already had their WebP.</summary>
        public long AlreadyWebp { get; private set; }

        public long PngsToDelete { get; private set; }

        /// <summary>Gets the number of PNG thumbnails to convert that were handled: converted, damaged or failed.</summary>
        public long Processed => Interlocked.Read(ref this.processed);

        public long Converted => Interlocked.Read(ref this.converted);

        /// <summary>Gets the number of PNG thumbnails that do not decode, or are cut short.</summary>
        public long Damaged => Interlocked.Read(ref this.damaged);

        /// <summary>Gets the number of damaged PNG thumbnails whose original image was tried.</summary>
        public long DamagedHandled => Interlocked.Read(ref this.damagedHandled);

        /// <summary>Gets the number of damaged PNG thumbnails whose WebP was made from the original image.</summary>
        public long FromOriginals => Interlocked.Read(ref this.fromOriginals);

        public long Failed => Interlocked.Read(ref this.failed);

        public long Deleted => Interlocked.Read(ref this.deleted);

        public long PngBytes => Interlocked.Read(ref this.pngBytes);

        public long WebpBytes => Interlocked.Read(ref this.webpBytes);

        public ConvertedThumbnail Last { get; private set; }

        public TimeSpan Elapsed => this.elapsed.Elapsed;

        public double FilesPerSecond =>
            this.converting.Elapsed.TotalSeconds < 1 ? 0 : this.Processed / this.converting.Elapsed.TotalSeconds;

        public TimeSpan? Remaining
        {
            get
            {
                var rate = this.FilesPerSecond;
                var left = this.Total - this.Processed;
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

        public void StartRecreating()
        {
            this.converting.Stop();
            this.Phase = WebpConversionPhase.RecreatingFromOriginals;
        }

        public void StartDeleting(long count)
        {
            this.converting.Stop();
            this.PngsToDelete = count;
            this.Phase = WebpConversionPhase.DeletingPngs;
        }

        public void SetNote(string note) => this.Note = note;

        public void AddConverted(string pngKey, long pngLength, long webpLength)
        {
            Interlocked.Increment(ref this.converted);
            Interlocked.Increment(ref this.processed);
            Interlocked.Add(ref this.pngBytes, pngLength);
            Interlocked.Add(ref this.webpBytes, webpLength);
            this.Last = new ConvertedThumbnail(pngKey, pngLength, webpLength);
        }

        public void AddDamaged()
        {
            Interlocked.Increment(ref this.damaged);
            Interlocked.Increment(ref this.processed);
        }

        public void AddConversionFailed(string key, string message)
        {
            Interlocked.Increment(ref this.processed);
            this.AddFailed(key, message);
        }

        public void AddRecreated()
        {
            Interlocked.Increment(ref this.fromOriginals);
            Interlocked.Increment(ref this.damagedHandled);
        }

        public void AddNotRecreated(string key, string message)
        {
            Interlocked.Increment(ref this.damagedHandled);
            this.AddFailed(key, message);
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
