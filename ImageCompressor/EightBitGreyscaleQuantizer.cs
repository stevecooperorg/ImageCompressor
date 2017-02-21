using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class EightBitGreyscaleQuantizer
{
    public Bitmap Quantize(Bitmap source)
    {
        var bounds = new Rectangle(0, 0, source.Width, source.Height);

        // Convert the source image into a color (ARGB) bitmap by
        // copying to a 32-bit (4-byte) pic;
        var colorBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(colorBitmap))
        {
            g.PageUnit = GraphicsUnit.Pixel;
            g.DrawImage(source, bounds);
        }

        // Make a greyscale result with 256 palette items;
        // item 0 is black -> item 255 is white.
        var greyBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format8bppIndexed);
        var palette = greyBitmap.Palette;
        for (var i = 0; i < 256; i++)
        {
            palette.Entries[i] = Color.FromArgb(255, i, i, i);
        }
        greyBitmap.Palette = palette;
        
        // fast read/write interface of color / grey bitmaps.
        BitmapData colorData = colorBitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData greyData = greyBitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

        // Define the source data pointers. The source row is a byte to
        // keep addition of the stride value easier (as this is in bytes)
        var colorPixel = colorData.Scan0;

        // Now define the destination data pointers
        var greyPixel = greyData.Scan0;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var r = Marshal.ReadByte(colorPixel, 1);
                var g = Marshal.ReadByte(colorPixel, 2);
                var b = Marshal.ReadByte(colorPixel, 3);
                var greyValue = (byte)(0.5 + r * 0.299 + g * 0.587 + b * 0.114); 
                
                Marshal.WriteByte(greyPixel, greyValue);

                colorPixel = colorPixel + 4; // advance 4 bytes (ARGB)
                greyPixel = greyPixel + 1; // advance 1 byte in greyscale
            }            
        }

        greyBitmap.UnlockBits(greyData);
        colorBitmap.UnlockBits(colorData);
        
        return greyBitmap;
    }
}