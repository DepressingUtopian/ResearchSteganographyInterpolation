using System;
using System.IO;
using System.Drawing;

using ImageWorker;

namespace StegoAnalysys
{
  public class VisualAttack
  {
    private const string OUT_PATH_ATTACK = @"C:\Users\Xen\Documents\StegoResearch\Code\results\visual";
    private static int computePixelAttack(int value, int numSelectedBit)
    {
      return (value & (int)(1 << numSelectedBit - 1)) > 0 ? 255 : 0;
    }
    private static Color computePixelAttackV2(
      Color pixel,
      int numSelectedBit,
      bool isRed,
      bool isGreen,
      bool isBlue)
    {
      if (isRed)
      {
        if ((pixel.R & (int)(1 << numSelectedBit - 1)) > 0)
          return Color.FromArgb(255, 255, 255);
      }

      if (isGreen)
      {
        if ((pixel.G & (int)(1 << numSelectedBit - 1)) > 0)
          return Color.FromArgb(255, 255, 255);
      }

      if (isBlue)
      {
        if ((pixel.B & (int)(1 << numSelectedBit - 1)) > 0)
          return Color.FromArgb(255, 255, 255);
      }
      return Color.FromArgb(0, 0, 0);
    }
    public static void getPixelComponentToFile(
      string imagePath, 
      bool isRed = false,
      bool isGreen = false,
      bool isBlue = false,
      bool isRandom = false,
      int numSelectedBit = 8
    )
    {
      ImageBuilder imageBuilder = new ImageBuilder(imagePath);
      ImageBuilder resultImage = new ImageBuilder(imageBuilder.Bitmap.Width, imageBuilder.Bitmap.Height);
      Random random = new Random();

      for (int x = 0; x < imageBuilder.Bitmap.Width; x++)
        for (int y = 0; y < imageBuilder.Bitmap.Height; y++)
        {
          Color origPixel = imageBuilder.Bitmap.GetPixel(x, y);
          Color newPixel = Color.FromArgb(0, 0, 0, 0);

          if (isRandom)
          {
            newPixel = Color.FromArgb(
            origPixel.A,
            random.Next(0, 1) > 0 ? computePixelAttack(origPixel.R, numSelectedBit) : 0,
            random.Next(0, 1) > 0 ? computePixelAttack(origPixel.G, numSelectedBit) : 0,
            random.Next(0, 1) > 0 ? computePixelAttack(origPixel.B, numSelectedBit) : 0
            );
          }
          else
          {
            newPixel = computePixelAttackV2(origPixel, numSelectedBit, isRed, isGreen, isBlue);
          }

          imageBuilder.Bitmap.SetPixel(x, y, newPixel);
        }

      imageBuilder.Save(OUT_PATH_ATTACK, Path.GetFileName(imagePath));
    }
  }
}
