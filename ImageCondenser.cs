using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;

class ImageCondenser
{
    public static void CreateCondensedFile(string zipFilePath, string condensedFilePath)
    {
        if (File.Exists(condensedFilePath))
        {
            File.Delete(condensedFilePath);
        }

        using (FileStream fs = new FileStream(condensedFilePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            string tempFolder = "temp_unpacked";

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }

            Directory.CreateDirectory(tempFolder);

            ZipFile.ExtractToDirectory(zipFilePath, tempFolder);

            string[] imageFiles = Directory.GetFiles(tempFolder, "*.jpg");

            writer.Write(imageFiles.Length);

            foreach (string imageFile in imageFiles)
            {
                byte[] imageData = File.ReadAllBytes(imageFile);
                writer.Write(imageData.Length);
                writer.Write(imageData);
            }

            Directory.Delete(tempFolder, true);
        }

        Console.WriteLine("Condensed file created successfully.");
    }

    public static List<Bitmap> ExtractFromCondensedFile(string condensedFilePath)
    {
        List<Bitmap> images = new List<Bitmap>();

        using (FileStream fs = new FileStream(condensedFilePath, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            int imageCount = reader.ReadInt32();

            for (int i = 0; i < imageCount; i++)
            {
                int imageDataLength = reader.ReadInt32();
                byte[] imageData = reader.ReadBytes(imageDataLength);

                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    images.Add(new Bitmap(ms));
                }
            }
        }

        Console.WriteLine("Images extracted from condensed file successfully.");
        return images;
    }

    public static void CreateDistortedFile(List<Bitmap> images, string distortedFilePath, int imagesPerOriginal)
    {
        if (File.Exists(distortedFilePath))
        {
            File.Delete(distortedFilePath);
        }

        using (FileStream fs = new FileStream(distortedFilePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(images.Count);

            foreach (Bitmap image in images)
            {
                for (int i = 0; i < imagesPerOriginal; i++)
                {
                    using (Bitmap distortedImage = ImageDistorter.ApplyRandomDistortion(image))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            distortedImage.Save(ms, ImageFormat.Jpeg);
                            byte[] imageData = ms.ToArray();
                            writer.Write(imageData.Length);
                            writer.Write(imageData);
                        }
                    }
                }
            }
        }

        Console.WriteLine("Distorted .trnmt file created successfully.");
    }

    public static Color GetPixelFromDistortedFile(string distortedFilePath, int imageIndex, int x, int y)
    {
        using (FileStream fs = new FileStream(distortedFilePath, FileMode.Open))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            int imageCount = reader.ReadInt32();

            if (imageIndex >= imageCount)
            {
                throw new IndexOutOfRangeException("Image index out of range.");
            }

            for (int i = 0; i <= imageIndex; i++)
            {
                int imageDataLength = reader.ReadInt32();
                byte[] imageData = reader.ReadBytes(imageDataLength);

                if (i == imageIndex)
                {
                    using (MemoryStream ms = new MemoryStream(imageData))
                    using (Bitmap image = new Bitmap(ms))
                    {
                        return image.GetPixel(x, y);
                    }
                }
            }
        }

        throw new Exception("Unexpected error while retrieving pixel.");
    }
}
