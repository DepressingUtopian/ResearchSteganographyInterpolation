using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using ImageWorker;
using FileWorker;

namespace StegoAlgorithms
{
  public interface IStegoAlgorithm
  {
    int Encode(string stegoImagePath, string coveredFilePath, string outputPath)
    {
      return 0;
    }
    void Decode(string filepath, string outputPath, long bytesize) { }
  }
  // LSB - Простая реализация, запись в каждые младший бит
  public class LSB : IStegoAlgorithm
  {
    private const string OUT_PATH_ENCODE = @"C:\Users\Xen\Documents\StegoResearch\Code\results\lsb/encode";
    private const string OUT_PATH_DECODE = @"C:\Users\Xen\Documents\StegoResearch\Code\results\lsb/decode";
    static public int Encode(string stegoImagePath, string coveredFilePath, string outputPath)
    {
      ImageBuilder imageBuilder = new ImageBuilder(stegoImagePath);
      FileBuilder fileBuilder = new FileBuilder(coveredFilePath);

      Bitmap imageBitmap = new Bitmap(imageBuilder.Bitmap);
      int totalPixels = imageBitmap.Width * imageBitmap.Height;

      if (fileBuilder.Data.Count * 8 > totalPixels * 3)
      {
        throw new Exception(String.Format("Не возможно внедрить файл в изображение, не хватает {0} бит", fileBuilder.Data.Count * 8 - totalPixels * 3));
      }

      int fileByteIter = 0;
      byte fileByte = (byte)fileBuilder.Data[fileByteIter];
      for (int x = 0; x < imageBitmap.Width; x++)
      {
        int prevByteIter = 0;

        if (fileByteIter > fileBuilder.Data.Count - 1)
          break;

        for (int y = 0; y < imageBitmap.Height; y++)
        {

          if (fileByteIter > fileBuilder.Data.Count - 1)
            break;

          if (prevByteIter != fileByteIter)
            fileByte = (byte)fileBuilder.Data[fileByteIter];

          for (int k = 0; k < 3; k++)
          {
            Color pixel = imageBitmap.GetPixel(x, y);

            byte r = (byte)((pixel.R & ~1) | (fileByte & 1));
            byte g = (byte)((pixel.G & ~1) | ((fileByte & 2) >> 1));
            byte b = pixel.B;
            if (k != 2)
              b = (byte)((pixel.B & ~1) | ((fileByte & 4) >> 2));

            fileByte = (byte)(fileByte >> 3);

            imageBitmap.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
            y++;
          }

          fileByteIter++;
        }
      }

      imageBuilder.Bitmap = imageBitmap;
      imageBuilder.Save(OUT_PATH_ENCODE);
      return fileBuilder.Data.Count;
    }

    static public void Decode(string filepath, string filename, long bytesize)
    {
      ImageBuilder imageBuilder = new ImageBuilder(filepath);
      FileBuilder fileBuilder = new FileBuilder();
      fileBuilder.Filename = filename;

      Bitmap imageBitmap = imageBuilder.Bitmap;
      int totalPixels = imageBitmap.Width * imageBitmap.Height;

      if (bytesize * 8 > totalPixels * 3)
      {
        throw new Exception("Не возможно извлечь файл из изображения");
      }

      int fileByteIter = 0;
      byte fileByte = 0;
      for (int x = 0; x < imageBitmap.Width; x++)
      {
        int prevByteIter = 0;

        for (int y = 0; y < imageBitmap.Height; y++)
        {
          fileByte = 0;

          for (int k = 0; k < 3; k++)
          {
            Color pixel = imageBitmap.GetPixel(x, y);
            fileByte |= (byte)(((pixel.G & 1) << (1 + k * 3)) | ((pixel.R & 1) << (k * 3)));
            if (k != 2)
              fileByte |= (byte)((pixel.B & 1) << (2 + k * 3));
            y++;
          }
          fileBuilder.Data.Add(fileByte);
          if (fileBuilder.Data.Count >= bytesize)
          {
            fileBuilder.Save(OUT_PATH_DECODE);
            return;
          }
          fileByteIter++;
        }
      }
    }
  }

  public class StegoInterpolation : IStegoAlgorithm
  {
    private const string OUT_PATH_ENCODE = @"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode";
    private const string OUT_PATH_DECODE = @"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\decode";
    static private byte Interpolate(params byte[] values)
    {
      byte result = 0;
      for (int i = 0; i < values.Length; i++)
      {
        result += (byte)(values[i] / values.Length);
      }

      return result;
    }

    static private byte[] GetByteArrayFromPixels(params Color[] pixels)
    {
      List<byte> result = new List<byte>();
      foreach (Color elem in pixels)
      {
        result.Add(elem.R);
        result.Add(elem.G);
        result.Add(elem.B);
      }

      return result.ToArray();
    }
    static public int Encode(string stegoImagePath, string coveredFilePath)
    {
      ImageBuilder imageBuilder = new ImageBuilder(stegoImagePath);
      FileBuilder fileBuilder = new FileBuilder(coveredFilePath);

      Bitmap imageBitmap = new Bitmap(imageBuilder.Bitmap);
      int totalPixels = imageBitmap.Width * imageBitmap.Height;

      if (fileBuilder.Data.Count * 8 > totalPixels)
      {
        throw new Exception(String.Format("Не возможно внедрить файл в изображение, не хватает {0} бит", fileBuilder.Data.Count * 8 - totalPixels * 3));
      }

      int fileIterIndex = 0;
      for (int x = 0; x < imageBitmap.Width; x++)
      {
        if (fileIterIndex >= fileBuilder.Data.Count)
          break;


        for (int y = 0; y < imageBitmap.Height; y++)
        {
          if (fileIterIndex >= fileBuilder.Data.Count)
            break;

          Color resultPixel = new Color();
          byte fileByte = (byte)fileBuilder.Data[fileIterIndex++];

          //В одном байте 8 бит!
          for (int i = 0; i < 8; i++)
          {
            byte bit = (byte)(fileByte & 1);
            fileByte = (byte)(fileByte >> 1);

            if (y >= imageBitmap.Height)
            {
              x++;
              y = 0;
            }

            Color pixel = imageBitmap.GetPixel(x, y);
            Color computePixel = Color.FromArgb(pixel.A, pixel.R, 0, pixel.B);
            Color nextNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            Color prevNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            int newInterValue = 0;

            //Если сверху нет соседнего пикселя
            if (y - 1 < 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, nextNeighbourPixel));
            }

            //Если снизу нет соседнего пикселя
            if (y - 1 > 0 && y + 1 >= imageBitmap.Height)
            {
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, prevNeighbourPixel));
            }

            //Если есть сосед сверху и снизу
            if (y - 1 > 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, nextNeighbourPixel, prevNeighbourPixel));
            }

            resultPixel = Color.FromArgb(pixel.A, pixel.R, newInterValue + bit, pixel.B);
            imageBitmap.SetPixel(x, y, resultPixel);
            y += 2;
          }
        }
      }

      imageBuilder.Bitmap = imageBitmap;
      // imageBuilder.Save(OUT_PATH_ENCODE);
      return fileBuilder.Data.Count;
    }

    static public void Decode(string filepath, string filename, long bytesize)
    {
      ImageBuilder imageBuilder = new ImageBuilder(filepath);
      FileBuilder fileBuilder = new FileBuilder();
      fileBuilder.Filename = filename;

      Bitmap imageBitmap = new Bitmap(imageBuilder.Bitmap);
      int totalPixels = imageBitmap.Width * imageBitmap.Height;

      if (bytesize * 8 > totalPixels)
      {
        throw new Exception("Не возможно извлечь файл из изображения");
      }

      for (int x = 0; x < imageBitmap.Width; x++)
      {
        if (fileBuilder.Data.Count >= bytesize)
          break;

        for (int y = 0; y < imageBitmap.Height; y++)
        {
          if (fileBuilder.Data.Count >= bytesize)
            break;

          byte fileByte = 0;

          //В одном байте 8 бит!
          for (int i = 0; i < 8; i++)
          {
            if (y >= imageBitmap.Height)
            {
              x++;
              y = 0;
            }

            Color originalPixel = imageBitmap.GetPixel(x, y);
            Color computePixel = Color.FromArgb(originalPixel.A, originalPixel.R, 0, originalPixel.B);
            Color nextNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            Color prevNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            int newInterValue = 0;

            //Если сверху нет соседнего пикселя
            if (y - 1 < 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, nextNeighbourPixel));
            }

            //Если снизу нет соседнего пикселя
            if (y - 1 > 0 && y + 1 >= imageBitmap.Height)
            {
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, prevNeighbourPixel));
            }

            //Если есть сосед сверху и снизу
            if (y - 1 > 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
              newInterValue = Interpolate(GetByteArrayFromPixels(computePixel, nextNeighbourPixel, prevNeighbourPixel));
            }

            fileByte |= (byte)(((originalPixel.G - newInterValue) & 1) << i);
            y += 2;
          }

          fileBuilder.Data.Add(fileByte);
        }
      }

      fileBuilder.Save(OUT_PATH_DECODE);
    }
  }
}
