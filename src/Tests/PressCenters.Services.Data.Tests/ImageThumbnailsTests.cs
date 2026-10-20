namespace PressCenters.Services.Data.Tests
{
    using System;
    using System.Buffers.Binary;
    using System.IO;
    using System.Linq;
    using System.Text;

    using PressCenters.Services.Data.Images;

    using SkiaSharp;

    using Xunit;

    public class ImageThumbnailsTests
    {
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        [Theory]
        [InlineData(SKEncodedImageFormat.Jpeg)]
        [InlineData(SKEncodedImageFormat.Png)]
        [InlineData(SKEncodedImageFormat.Webp)]
        public void ShrinkToWidthShouldScaleDownKeepingTheAspectRatio(SKEncodedImageFormat format)
        {
            using var thumbnail = DecodePng(ImageThumbnails.ShrinkToWidth(CreateImage(1460, 1000, format), 730));

            Assert.Equal(730, thumbnail.Width);
            Assert.Equal(500, thumbnail.Height);
        }

        [Fact]
        public void ShrinkToWidthShouldNotEnlargeSmallImages()
        {
            using var thumbnail = DecodePng(
                ImageThumbnails.ShrinkToWidth(CreateImage(400, 300, SKEncodedImageFormat.Jpeg), 730));

            Assert.Equal(400, thumbnail.Width);
            Assert.Equal(300, thumbnail.Height);
        }

        [Theory]
        [InlineData(4000, 3000)]
        [InlineData(1000, 400)]
        [InlineData(300, 900)]
        [InlineData(50, 30)]
        public void CropToFillShouldReturnExactlyTheRequestedSize(int width, int height)
        {
            using var thumbnail = DecodePng(
                ImageThumbnails.CropToFill(CreateImage(width, height, SKEncodedImageFormat.Jpeg), 200, 120));

            Assert.Equal(200, thumbnail.Width);
            Assert.Equal(120, thumbnail.Height);
        }

        [Fact]
        public void CropToFillShouldKeepTheCenter()
        {
            // Red | green | blue stripes: filling a square keeps only the green middle.
            using var source = new SKBitmap(300, 100);
            using (var canvas = new SKCanvas(source))
            using (var paint = new SKPaint())
            {
                canvas.Clear(SKColors.Red);
                paint.Color = SKColors.Lime;
                canvas.DrawRect(SKRect.Create(100, 0, 100, 100), paint);
                paint.Color = SKColors.Blue;
                canvas.DrawRect(SKRect.Create(200, 0, 100, 100), paint);
            }

            using var thumbnail = DecodePng(ImageThumbnails.CropToFill(Encode(source, SKEncodedImageFormat.Png), 100, 100));

            Assert.Equal(SKColors.Lime, thumbnail.GetPixel(0, 0));
            Assert.Equal(SKColors.Lime, thumbnail.GetPixel(50, 50));
            Assert.Equal(SKColors.Lime, thumbnail.GetPixel(99, 99));
        }

        [Fact]
        public void ThumbnailsShouldKeepTransparency()
        {
            using var source = new SKBitmap(new SKImageInfo(400, 240, SKColorType.Rgba8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(source))
            using (var paint = new SKPaint { Color = SKColors.Red })
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawRect(SKRect.Create(100, 60, 200, 120), paint);
            }

            using var thumbnail = DecodePng(ImageThumbnails.CropToFill(Encode(source, SKEncodedImageFormat.Png), 200, 120));

            Assert.Equal(0, thumbnail.GetPixel(0, 0).Alpha);
            Assert.Equal(SKColors.Red, thumbnail.GetPixel(100, 60));
        }

        [Fact]
        public void GifsAndGrayscaleJpegsShouldBeDecoded()
        {
            var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            using var gray = new SKBitmap(new SKImageInfo(300, 200, SKColorType.Gray8, SKAlphaType.Opaque));
            gray.Erase(SKColors.Gray);

            using var gifThumbnail = DecodePng(ImageThumbnails.CropToFill(gif, 200, 120));
            using var grayThumbnail = DecodePng(ImageThumbnails.ShrinkToWidth(Encode(gray, SKEncodedImageFormat.Jpeg), 730));

            Assert.Equal(200, gifThumbnail.Width);
            Assert.Equal(300, grayThumbnail.Width);
        }

        [Fact]
        public void LargeJpegsShouldBeDecodedAtAReducedScale()
        {
            // 52 megapixels is over the decoding limit: this only works because the JPEG is decoded at 1/8.
            using var source = new SKBitmap(new SKImageInfo(8000, 6500, SKColorType.Rgba8888, SKAlphaType.Opaque));
            source.Erase(SKColors.SteelBlue);
            var jpeg = Encode(source, SKEncodedImageFormat.Jpeg);

            using var big = DecodePng(ImageThumbnails.ShrinkToWidth(jpeg, 730));
            using var small = DecodePng(ImageThumbnails.CropToFill(jpeg, 200, 120));

            Assert.Equal(730, big.Width);
            Assert.Equal(593, big.Height);
            Assert.Equal(200, small.Width);
        }

        [Theory]
        [InlineData("<!DOCTYPE html><html><body>404 Not Found</body></html>")]
        [InlineData("")]
        public void DataThatIsNotAnImageShouldThrowInvalidDataException(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);

            Assert.Throws<InvalidDataException>(() => ImageThumbnails.ShrinkToWidth(bytes, 730));
            Assert.Throws<InvalidDataException>(() => ImageThumbnails.CropToFill(bytes, 200, 120));
        }

        [Fact]
        public void ImagesThatDeclareHugeDimensionsShouldBeRefusedBeforeDecoding()
        {
            // A few bytes of PNG that declare 20000x20000 pixels: decoding them would need a 1.6 GB buffer.
            var png = CreatePngHeader(20000, 20000);

            var exception = Assert.Throws<InvalidDataException>(() => ImageThumbnails.CropToFill(png, 200, 120));
            Assert.Contains("too large", exception.Message);
        }

        private static byte[] CreateImage(int width, int height, SKEncodedImageFormat format)
        {
            using var bitmap = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(bitmap))
            using (var paint = new SKPaint { Color = SKColors.Orange })
            {
                canvas.Clear(SKColors.SteelBlue);
                canvas.DrawCircle(width / 2f, height / 2f, Math.Min(width, height) / 3f, paint);
            }

            return Encode(bitmap, format);
        }

        private static byte[] Encode(SKBitmap bitmap, SKEncodedImageFormat format)
        {
            using var pixmap = bitmap.PeekPixels();
            using var data = pixmap.Encode(format, 90);
            return data.ToArray();
        }

        private static SKBitmap DecodePng(byte[] png)
        {
            Assert.Equal(PngSignature, png.Take(PngSignature.Length));
            return SKBitmap.Decode(png);
        }

        // A PNG that declares width x height pixels but carries almost no image data.
        private static byte[] CreatePngHeader(int width, int height)
        {
            var header = new byte[13];
            BinaryPrimitives.WriteInt32BigEndian(header, width);
            BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
            header[8] = 8; // bit depth
            header[9] = 2; // RGB

            using var stream = new MemoryStream();
            stream.Write(PngSignature);
            WriteChunk(stream, "IHDR", header);
            WriteChunk(stream, "IDAT", new byte[] { 0x78, 0x9C, 0x63, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01 });
            WriteChunk(stream, "IEND", Array.Empty<byte>());
            return stream.ToArray();
        }

        private static void WriteChunk(Stream stream, string type, byte[] data)
        {
            var typeAndData = Encoding.ASCII.GetBytes(type).Concat(data).ToArray();
            var buffer = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buffer, data.Length);
            stream.Write(buffer);
            stream.Write(typeAndData);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, Crc32(typeAndData));
            stream.Write(buffer);
        }

        private static uint Crc32(byte[] bytes)
        {
            var crc = 0xFFFFFFFFu;
            foreach (var value in bytes)
            {
                crc ^= value;
                for (var bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
                }
            }

            return ~crc;
        }
    }
}
