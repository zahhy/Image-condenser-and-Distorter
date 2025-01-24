using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;

class ImageDistorter
{
    static Random random = new Random();

    static void Main(string[] args)
    {
        string zipFilePath = "input_images.zip";
        string condensedFilePath = "condensedimages.trnmt";
        string distortedFilePath = "distortedimages.trnmt";

        Unpacker.CreateCondensedFile(zipFilePath, condensedFilePath);

        List<Bitmap> unpackedImages = Unpacker.ExtractFromCondensedFile(condensedFilePath);

        Unpacker.CreateDistortedFile(unpackedImages, distortedFilePath, 10);

        Console.WriteLine("Distorted images generated successfully into .trnmt file.");
    }

    static Bitmap ApplyRandomDistortion(Bitmap originalImage)
    {
        Bitmap distortedImage = new Bitmap(originalImage.Width, originalImage.Height);

        using (Graphics g = Graphics.FromImage(distortedImage))
        {
            int offsetX = random.Next(-5, 5);
            int offsetY = random.Next(-5, 5);

            float angle = random.Next(-10, 10);
            g.TranslateTransform(originalImage.Width / 2, originalImage.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-originalImage.Width / 2, -originalImage.Height / 2);

            float scaleX = (float)(0.95 + random.NextDouble() * 0.1);
            float scaleY = (float)(0.95 + random.NextDouble() * 0.1);
            g.ScaleTransform(scaleX, scaleY);

            g.DrawImage(originalImage, offsetX, offsetY, originalImage.Width, originalImage.Height);

            for (int x = 0; x < distortedImage.Width; x++)
            {
                for (int y = 0; y < distortedImage.Height; y++)
                {
                    if (random.NextDouble() < 0.03)
                    {
                        Color pixelColor = distortedImage.GetPixel(x, y);
                        int r = Clamp(pixelColor.R + random.Next(-30, 30));
                        int gVal = Clamp(pixelColor.G + random.Next(-30, 30));
                        int b = Clamp(pixelColor.B + random.Next(-30, 30));

                        distortedImage.SetPixel(x, y, Color.FromArgb(r, gVal, b));
                    }
                }
            }

            g.ResetTransform();
        }

        return distortedImage;
    }

    static int Clamp(int value)
    {
        return Math.Max(0, Math.Min(255, value));
    }
}