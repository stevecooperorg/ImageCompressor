using System;
using System.Collections.Generic;
using System.Drawing; // include System.Drawing.dll
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Compresses bitmaps to an approximate size
/// </summary>
public class ImageSizeOptimizer
{
    /// <summary>
    /// How big will this file be as PNG bytes?
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public long SizeOf(Bitmap bitmap, ImageFormat format)
    {
        using (var memoryStream = new MemoryStream())
        {
            bitmap.Save(memoryStream, format);
            return memoryStream.Length;
        }
    }

    /// <summary>
    /// Return a bitmap of about the right size (to within 5%)
    /// </summary>
    /// <param name="original">the bitmap to scale</param>
    /// <param name="targetSize">the target size, in bytes</param>
    /// <param name="format">PNG, GIF, JPEG, etc</param>
    /// <returns>the resized image</returns>
    public Bitmap AutoScale(Bitmap original, long targetSize, ImageFormat format, bool greyscale)
    {
        Console.WriteLine($"Auto-scaling to {targetSize:n0}b");
        var result = AutoScale(original, targetSize, format, 1, 2, greyscale);
        Console.WriteLine($"Scaled to {result.Width} wide, {result.Height} high");
        return result;
    }

    /// <summary>
    /// recursive function to seek the optimal size
    /// </summary>
    private Bitmap AutoScale(Bitmap original, long targetSize, ImageFormat format, int numerator, int divisor, bool greyscale)
    {
        var currentScaleFactor = 1.0d * numerator / divisor;
        var current = Scale(original, currentScaleFactor);

        if (greyscale)
        {
            current = new EightBitGreyscaleQuantizer().Quantize(current);
        }
        var currentFileSize = SizeOf(current, format);
        Console.WriteLine($"Auto-scaling to {targetSize:n0}b using scale factor {numerator}/{divisor} - got {currentFileSize:n0} bytes");

        var diff = currentFileSize - targetSize;
        var abs = Math.Abs(diff);
        var allowedError = targetSize * 0.05; // 5% error allowed
        var sign = Math.Sign(diff);

        if (abs < allowedError)
        {
            return current;
        }
        else if (sign == 1)
        {
            //  too big;
            return AutoScale(original, targetSize, format, numerator * 2 - 1, divisor * 2, greyscale);
        }
        else
        {
            // too small;
            return AutoScale(original, targetSize, format, numerator * 2 + 1, divisor * 2, greyscale);
        }
    }

    /// <summary>
    /// high-quality resize function
    /// </summary>
    /// <param name="image"></param>
    /// <param name="scaleFactor"></param>
    /// <returns></returns>
    private Bitmap Scale(Bitmap image, double scaleFactor)
    {
        var width = (int)(image.Width * scaleFactor);
        var height = (int)(image.Height * scaleFactor);
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
    }
}