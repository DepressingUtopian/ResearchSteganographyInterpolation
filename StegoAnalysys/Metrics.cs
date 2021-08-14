using System;
using System.IO;
using System.Drawing;

using ImageWorker;

namespace StegoAnalysys
{
  public class Metrics
  {
    public static double PSNR(String origImagePath, String modifiedImagePath)
    {
      ImageBuilder origImage = new ImageBuilder(origImagePath);
      ImageBuilder modifiedImage = new ImageBuilder(modifiedImagePath);

      double mse = MSE(origImage, modifiedImage);
      return 10 * Math.Log10(255 * 255 / mse);

    }

    private static double pixelsDiff(Color pixel1, Color pixel2)
    {
      return Math.Abs((pixel1.R - pixel2.R) + (pixel1.G - pixel2.G) + (pixel1.B - pixel2.B)) / 3;
    }

    public static double MSE(ImageBuilder origImage, ImageBuilder modifiedImage)
    {
      double diff = 0;
      int M = origImage.Bitmap.Width;
      int N = origImage.Bitmap.Height;

      for (int j = 0; j < M; j++)
        for (int k = 0; k < N; k++)
        {
          double error = pixelsDiff(origImage.Bitmap.GetPixel(j, k), modifiedImage.Bitmap.GetPixel(j, k));
          diff += error * error;
        }
      return diff / (double)(M * N);
    }

    private static Color multPixel(Color pixel1, Color pixel2) {
      return Color.FromArgb(pixel1.A, pixel1.R * pixel2.R, pixel1.G * pixel2.G, pixel1.B * pixel2.B);
    }

    private static double multPixelD(Color pixel1, Color pixel2) {
      return (pixel1.R * pixel2.R + pixel1.G * pixel2.G + pixel1.B * pixel2.B) / 3;
    }

    public static double IF(String origImagePath, String modifiedImagePath)
    {
      ImageBuilder origImage = new ImageBuilder(origImagePath);
      ImageBuilder modifiedImage = new ImageBuilder(modifiedImagePath);

      double diff1 = 0;
      double diff2 = 0;
      int M = origImage.Bitmap.Width;
      int N = origImage.Bitmap.Height;

      for (int j = 0; j < M; j++)
        for (int k = 0; k < N; k++)
        {
          double error = pixelsDiff(modifiedImage.Bitmap.GetPixel(j, k), origImage.Bitmap.GetPixel(j, k));
          diff1 += error * error;
        }

      for (int j = 0; j < M; j++)
        for (int k = 0; k < N; k++)
        {
          double error = multPixelD(origImage.Bitmap.GetPixel(j, k), origImage.Bitmap.GetPixel(j, k));
          diff2 += error * error;
        }

      return 1.0d - (diff1 / diff2);
    }

    // Среднее значение пикселя
    private static (double R, double G, double B) middleValue(ImageBuilder image)
    {
      (double R, double G, double B) result = (0, 0, 0);
      int M = image.Bitmap.Width;
      int N = image.Bitmap.Height;
      for (int j = 0; j < M; j++)
        for (int k = 0; k < N; k++)
        {
          Color pixel = image.Bitmap.GetPixel(j, k);
          result.R += (double)pixel.R / (double)(M * N);
          result.G += (double)pixel.G / (double)(M * N);
          result.B += (double)pixel.B / (double)(M * N);
        }
      return result;
    }

    private static double sigma(ImageBuilder image)
    {
      double result = 0;
      int M = image.Bitmap.Width;
      int N = image.Bitmap.Height;
      var mid = middleValue(image);
      Color middlePixel = Color.FromArgb(0, (int)mid.R, (int)mid.G, (int)mid.B);
      for (int i = 0; i < M - 1; i++)
        for (int j = 0; j < N - 1; j++)
        {
          Color pixel = image.Bitmap.GetPixel(i, j);
          result += pixelsDiff(pixel, middlePixel) * pixelsDiff(pixel, middlePixel);
        }
      return Math.Sqrt(result);
    }

     private static double sigmaCS(ImageBuilder C, ImageBuilder S)
    {
      double result = 0;
      int M = C.Bitmap.Width;
      int N = C.Bitmap.Height;
      var midC = middleValue(C);
      var midS = middleValue(S);
      Color middlePixelC = Color.FromArgb(0, (int)midC.R, (int)midC.G, (int)midC.B);
      Color middlePixelS = Color.FromArgb(0, (int)midS.R, (int)midS.G, (int)midS.B);

      for (int i = 0; i < M - 1; i++)
        for (int j = 0; j < N - 1; j++)
        {
          Color pixelC = C.Bitmap.GetPixel(i, j);
          Color pixelS = S.Bitmap.GetPixel(i, j);
          result += pixelsDiff(pixelC, middlePixelC) * pixelsDiff(pixelS, middlePixelS);
        }
      return result;
    }

    public static double NCC(String origImagePath, String modifiedImagePath)
    {
      ImageBuilder origImage = new ImageBuilder(origImagePath);
      ImageBuilder modifiedImage = new ImageBuilder(modifiedImagePath);

      return (sigmaCS(origImage, modifiedImage) / (sigma(origImage) * sigma(modifiedImage)));
    }

    public static double Q(String origImagePath, String modifiedImagePath)
    {
      ImageBuilder origImage = new ImageBuilder(origImagePath);
      ImageBuilder modifiedImage = new ImageBuilder(modifiedImagePath);
      var middleC = middleValue(origImage);
      var middleS = middleValue(modifiedImage);

      double middleCoef = ((middleC.R * middleS.R + middleC.G * middleS.G + middleC.B * middleS.B) / 3) / 
        (((middleC.R * middleC.R + middleC.G * middleC.G + middleC.B * middleC.B) / 3) + ((middleS.R * middleS.R + middleS.G * middleS.G + middleS.B * middleS.B) / 3));

      double sigmaCoef = (sigmaCS(origImage, modifiedImage) / (sigma(origImage) * sigma(origImage) + sigma(modifiedImage) * sigma(modifiedImage)));

      return 4 * middleCoef * sigmaCoef;
    }
  }
}