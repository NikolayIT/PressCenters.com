namespace Sandbox.Code
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Spectre.Console;
    using Spectre.Console.Rendering;

    /// <summary>The live console view of <see cref="ConvertThumbnailsToWebpSandbox"/> (and its one-line fallback).</summary>
    public static class WebpConversionDashboard
    {
        private const int BarWidth = 50;

        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static IRenderable Render(WebpConversionProgress progress, string target, string optionsMarkup)
        {
            var rows = new List<IRenderable>
                       {
                           new Markup($"[grey]Storage[/]  [deepskyblue1]{Markup.Escape(target)}[/]"),
                           new Markup($"[grey]Options[/]  {optionsMarkup}"),
                           Text.Empty,
                           PhaseLine(progress),
                       };
            if (progress.Phase != WebpConversionPhase.Listing)
            {
                rows.Add(progress.Phase == WebpConversionPhase.DeletingPngs
                    ? Bar(Fraction(progress.Deleted, progress.PngsToDelete), "orange1")
                    : Bar(Fraction(progress.Converted + progress.Failed, progress.Total), "springgreen2"));
            }

            rows.Add(Text.Empty);
            rows.Add(Stats(progress));
            if (progress.PngBytes > 0)
            {
                var saved = Math.Max(0, progress.PngBytes - progress.WebpBytes);
                rows.Add(Text.Empty);
                rows.Add(new BreakdownChart()
                    .Width(BarWidth + 9)
                    .AddItem($"WebP {Bytes(progress.WebpBytes)}", progress.WebpBytes, Color.SpringGreen2)
                    .AddItem($"saved {Bytes(saved)} ({Percent(saved, progress.PngBytes)})", saved, Color.Gold1)
                    .HideTagValues());
            }

            if (progress.Last is { } last)
            {
                rows.Add(new Markup(
                    $"[grey]Last[/]  {Markup.Escape(last.PngKey)}  [grey]{Bytes(last.PngBytes)} →[/] [springgreen2]{Bytes(last.WebpBytes)}[/]"));
            }

            foreach (var error in progress.RecentErrors)
            {
                rows.Add(new Markup($"[red]× {Markup.Escape(error)}[/]"));
            }

            rows.Add(Text.Empty);
            rows.Add(new Markup("[grey]Ctrl+C stops after the files in progress; running again resumes where it stopped.[/]"));

            return new Panel(new Rows(rows))
                .Header("[bold orange1] PressCenters [/][grey]·[/][bold white] news thumbnails PNG → WebP [/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Padding(2, 1, 2, 1);
        }

        public static string StatusLine(WebpConversionProgress progress) => progress.Phase switch
        {
            WebpConversionPhase.Listing => $"Listing: {progress.Listed.ToString("N0", Culture)} keys",
            WebpConversionPhase.DeletingPngs =>
                $"Deleting PNGs that have a WebP: {progress.Deleted.ToString("N0", Culture)}/{progress.PngsToDelete.ToString("N0", Culture)}",
            WebpConversionPhase.Failed => $"Failed: {progress.Error}",
            _ => $"{progress.Phase}: {(progress.Converted + progress.Failed).ToString("N0", Culture)}/{progress.Total.ToString("N0", Culture)}"
                 + $" ({Percent(progress.Converted + progress.Failed, progress.Total)}), converted {progress.Converted.ToString("N0", Culture)},"
                 + $" failed {progress.Failed.ToString("N0", Culture)}, already WebP {progress.AlreadyWebp.ToString("N0", Culture)},"
                 + $" PNGs deleted {progress.Deleted.ToString("N0", Culture)}, {progress.FilesPerSecond.ToString("N1", Culture)} files/s,"
                 + $" {Bytes(progress.PngBytes)} -> {Bytes(progress.WebpBytes)}, elapsed {Time(progress.Elapsed)}"
                 + (progress.Remaining is { } remaining ? $", remaining {Time(remaining)}" : string.Empty),
        };

        private static IRenderable PhaseLine(WebpConversionProgress progress) => progress.Phase switch
        {
            WebpConversionPhase.Listing =>
                new Markup($"[yellow]◌[/] [bold]Listing the storage[/]  [yellow]{progress.Listed.ToString("N0", Culture)}[/] [grey]keys so far[/]"),
            WebpConversionPhase.DeletingPngs =>
                new Markup($"[orange1]●[/] [bold]Deleting the PNGs that already have a WebP[/]  {Count(progress.Deleted)} [grey]of[/] {Count(progress.PngsToDelete)}"),
            WebpConversionPhase.Converting =>
                new Markup($"[springgreen2]●[/] [bold]Converting[/]  {Count(progress.Converted + progress.Failed)} [grey]of[/] {Count(progress.Total)}"),
            WebpConversionPhase.Done =>
                new Markup($"[springgreen2]√ Done[/]  {Count(progress.Converted)} [grey]converted,[/] {Count(progress.Failed)} [grey]failed[/]"),
            WebpConversionPhase.Stopped =>
                new Markup("[yellow]■ Stopped[/]  [grey]run again to continue where it stopped[/]"),
            _ => new Markup($"[red]× Failed:[/] {Markup.Escape(progress.Error ?? string.Empty)}"),
        };

        private static IRenderable Stats(WebpConversionProgress progress)
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().NoWrap().PadRight(1));
            grid.AddColumn(new GridColumn().NoWrap().RightAligned().PadRight(6));
            grid.AddColumn(new GridColumn().NoWrap().PadRight(1));
            grid.AddColumn(new GridColumn().NoWrap().RightAligned());
            grid.AddRow(
                "[springgreen2]√[/] Converted",
                $"[bold springgreen2]{Count(progress.Converted)}[/]",
                "[grey]PNG read[/]",
                Bytes(progress.PngBytes));
            grid.AddRow(
                "[deepskyblue1]»[/] Already WebP",
                $"[deepskyblue1]{Count(progress.AlreadyWebp)}[/]",
                "[grey]WebP written[/]",
                $"[springgreen2]{Bytes(progress.WebpBytes)}[/]");
            grid.AddRow(
                "[red]×[/] Failed",
                progress.Failed > 0 ? $"[bold red]{Count(progress.Failed)}[/]" : "[grey]0[/]",
                "[grey]Speed[/]",
                $"{progress.FilesPerSecond.ToString("N1", Culture)} [grey]files/s[/]");
            grid.AddRow(
                "[orange1]−[/] PNGs deleted",
                progress.Deleted > 0 ? $"[orange1]{Count(progress.Deleted)}[/]" : "[grey]0[/]",
                "[grey]Elapsed[/]",
                Time(progress.Elapsed));
            grid.AddRow(
                string.Empty,
                string.Empty,
                "[grey]Remaining[/]",
                progress.Remaining is { } remaining ? $"[yellow]{Time(remaining)}[/]" : "[grey]–[/]");
            return grid;
        }

        private static IRenderable Bar(double fraction, string color)
        {
            var filled = (int)Math.Round(Math.Clamp(fraction, 0, 1) * BarWidth);
            var done = filled > 0 ? $"[{color}]{new string('━', filled)}[/]" : string.Empty;
            var left = filled < BarWidth ? $"[grey27]{new string('━', BarWidth - filled)}[/]" : string.Empty;
            return new Markup($"{done}{left}  [bold]{(fraction * 100).ToString("0.0", Culture)}%[/]");
        }

        private static double Fraction(long done, long total) => total == 0 ? 1 : (double)done / total;

        private static string Percent(long part, long total) =>
            total == 0 ? "–" : (part * 100.0 / total).ToString("0.0", Culture) + "%";

        private static string Count(long value) => value.ToString("N0", Culture);

        private static string Time(TimeSpan time) =>
            time.TotalDays >= 1 ? time.ToString(@"d\.hh\:mm\:ss", Culture) : time.ToString(@"hh\:mm\:ss", Culture);

        private static string Bytes(long bytes) => bytes switch
        {
            >= 1L << 30 => $"{(bytes / (double)(1L << 30)).ToString("0.00", Culture)} GB",
            >= 1L << 20 => $"{(bytes / (double)(1L << 20)).ToString("0.0", Culture)} MB",
            >= 1L << 10 => $"{(bytes / (double)(1L << 10)).ToString("0.0", Culture)} KB",
            _ => $"{bytes.ToString(Culture)} B",
        };
    }
}
