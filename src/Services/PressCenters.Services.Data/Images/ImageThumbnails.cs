namespace PressCenters.Services.Data.Images
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using SkiaSharp;

    /// <summary>
    /// PNG thumbnails of downloaded images, made with SkiaSharp (Skia, Chrome's image stack). It decodes JPEG, PNG, GIF
    /// (first frame), WebP, BMP and ICO; anything else, or data that is not an image, throws
    /// <see cref="InvalidDataException"/>.
    /// </summary>
    public static class ImageThumbnails
    {
        // Refuses "decompression bombs" (a few KB that declare tens of thousands of pixels per side) before their pixel
        // buffer is allocated. Far above any real image here, all the more since JPEGs are decoded at a reduced scale.
        private const long MaxDecodedPixels = 50_000_000;

        /// <summary>Scales the image down to <paramref name="maxWidth"/>, keeping its aspect ratio (never up).</summary>
        /// <returns>The thumbnail as PNG.</returns>
        public static byte[] ShrinkToWidth(byte[] imageBytes, int maxWidth)
        {
            using var codec = CreateCodec(imageBytes);
            var size = codec.Info.Size;
            var width = Math.Min(maxWidth, size.Width);
            var height = Math.Max(1, (int)Math.Round((double)size.Height * width / size.Width));
            using var source = Decode(codec, width, height);
            return ResizeToPng(source, width, height);
        }

        /// <summary>
        /// Scales the image to cover <paramref name="width"/> x <paramref name="height"/> and crops what overflows,
        /// evenly from both sides.
        /// </summary>
        /// <returns>The thumbnail as PNG.</returns>
        public static byte[] CropToFill(byte[] imageBytes, int width, int height)
        {
            using var codec = CreateCodec(imageBytes);
            var size = codec.Info.Size;
            var scale = Math.Max((double)width / size.Width, (double)height / size.Height);
            using var source = Decode(
                codec,
                (int)Math.Ceiling(size.Width * scale),
                (int)Math.Ceiling(size.Height * scale));

            // The centered part of the decoded image that has the thumbnail's aspect ratio.
            var sourceScale = Math.Max((double)width / source.Width, (double)height / source.Height);
            var cropWidth = Math.Min(source.Width, Math.Max(1, (int)Math.Round(width / sourceScale)));
            var cropHeight = Math.Min(source.Height, Math.Max(1, (int)Math.Round(height / sourceScale)));
            var crop = SKRectI.Create(
                (source.Width - cropWidth) / 2,
                (source.Height - cropHeight) / 2,
                cropWidth,
                cropHeight);
            using var cropped = new SKBitmap();
            if (!source.ExtractSubset(cropped, crop))
            {
                throw new InvalidDataException("The image could not be cropped.");
            }

            return ResizeToPng(cropped, width, height);
        }

        private static SKCodec CreateCodec(byte[] imageBytes)
        {
            using var data = SKData.CreateCopy(imageBytes ?? Array.Empty<byte>());
            var codec = SKCodec.Create(data);
            if (codec == null || codec.Info.Width <= 0 || codec.Info.Height <= 0)
            {
                codec?.Dispose();
                throw new InvalidDataException("Not an image in a supported format.");
            }

            return codec;
        }

        // Decodes at the smallest size the codec can produce directly that is still at least minWidth x minHeight:
        // JPEG decodes at 1/8 steps and WebP at any size, so a large photo never needs a full-size pixel buffer.
        private static SKBitmap Decode(SKCodec codec, int minWidth, int minHeight)
        {
            var fullSize = codec.Info.Size;
            var scale = Math.Max((float)minWidth / fullSize.Width, (float)minHeight / fullSize.Height);

            // JPEG rounds the scale to the nearest 1/8: ask for 1/16 more so that it rounds up rather than down.
            var size = codec.GetScaledDimensions(Math.Min(1f, scale + (1f / 16)));
            if (size.Width < minWidth || size.Height < minHeight
                || size.Width > fullSize.Width || size.Height > fullSize.Height)
            {
                size = fullSize;
            }

            if ((long)size.Width * size.Height > MaxDecodedPixels)
            {
                throw new InvalidDataException($"The image is too large ({fullSize.Width}x{fullSize.Height}).");
            }

            var alphaType = codec.Info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
            var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, alphaType);
            return SKBitmap.Decode(codec, info) ?? throw new InvalidDataException("The image could not be decoded.");
        }

        // One cubic pass over a large reduction would alias, so halve with a box filter (linear sampling at exactly 1/2)
        // while the image is more than twice too large, then resample the rest with Catmull-Rom, the cubic ImageSharp
        // used. An image that already has the size is only encoded: a resampling pass would just blur it.
        private static byte[] ResizeToPng(SKBitmap source, int width, int height)
        {
            var intermediates = new List<SKBitmap>();
            try
            {
                var current = source;
                while (current.Width >= width * 2 && current.Height >= height * 2)
                {
                    current = Draw(current, current.Width / 2, current.Height / 2, new SKSamplingOptions(SKFilterMode.Linear));
                    intermediates.Add(current);
                }

                if (current.Width != width || current.Height != height)
                {
                    current = Draw(current, width, height, new SKSamplingOptions(SKCubicResampler.CatmullRom));
                    intermediates.Add(current);
                }

                using var pixmap = current.PeekPixels();
                using var png = pixmap.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 9));
                return png?.ToArray() ?? throw new InvalidDataException("The thumbnail could not be encoded.");
            }
            finally
            {
                intermediates.ForEach(x => x.Dispose());
            }
        }

        private static SKBitmap Draw(SKBitmap source, int width, int height, SKSamplingOptions sampling)
        {
            var target = new SKBitmap(new SKImageInfo(width, height, source.ColorType, source.AlphaType));
            using var canvas = new SKCanvas(target);
            canvas.Clear(SKColors.Transparent);

            // Immutable, so that the image shares the pixels instead of copying them.
            source.SetImmutable();
            using var image = SKImage.FromBitmap(source);
            canvas.DrawImage(image, SKRect.Create(width, height), sampling);
            return target;
        }
    }
}
