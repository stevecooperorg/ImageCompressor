using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCompressorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputFile = @"C:\src\ImageCompressor\Steve.Cooper.Passport.png";
            var targetFile = @"C:\src\ImageCompressor\Steve.Cooper.Passport.small.png";
            var bitmap = (Bitmap)System.Drawing.Bitmap.FromFile(inputFile);
            
            var compressor = new ImageSizeOptimizer();
            var result = compressor.AutoScale(bitmap, 2000 * 1000, ImageFormat.Png, greyscale: true);
            
            result.Save(targetFile, ImageFormat.Png);
            Console.WriteLine($"Compressed to {compressor.SizeOf(result, ImageFormat.Png):n0} bytes");
            
            Console.WriteLine("Done");
            Console.ReadLine();
        }      
    }  
}
