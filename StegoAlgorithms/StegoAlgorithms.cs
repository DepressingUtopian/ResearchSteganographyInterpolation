using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using ImageWorker;
using FileWorker;

namespace StegoAlgorithms
{

  public class FileIterator
  {

    public Boolean isEnd = false;
    public byte current = 0;
    public int iterCounter = 0;
    public int count = 0;

    private ArrayList data = null;
    public FileIterator(FileBuilder fileBuilder)
    {
      this.count = fileBuilder.Data.Count;
      this.data = fileBuilder.Data;
      this.goNext();
    }

    public byte goNext()
    {
      if (iterCounter < count)
      {
        this.current = (byte)data[iterCounter++];
        return this.current;
      }
      this.isEnd = true;
      return 0;
    }
  }
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
    private const string OUT_PATH_ENCODE = @"C:/Users/Xen/Documents/StegoResearch/Code/results/lsb/encode";
    private const string OUT_PATH_DECODE = @"C:/Users/Xen/Documents/StegoResearch/Code/results/lsb/decode";

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

  public abstract class StegoInterpolation
  {
    protected const string OUT_PATH_ENCODE = @"C:/Users/Xen/Documents/StegoResearch/Code/results/interpol/encode";
    protected const string OUT_PATH_DECODE = @"C:/Users/Xen/Documents/StegoResearch/Code/results/interpol/decode";
    static protected byte Interpolate(params byte[] values)
    {
      byte result = 0;
      for (int i = 0; i < values.Length; i++)
      {
        result += (byte)(values[i] / values.Length);
      }

      return result;
    }

    static protected byte[] GetByteArrayFromPixels(params Color[] pixels)
    {
      List<byte> result = new List<byte>();
      foreach (Color elem in pixels)
      {
        result.Add(elem.B);
      }

      return result.ToArray();
    }

    static protected Color Interpolate(int origA, params Color[] pixels)
    {
      (int R, int G, int B) result = (0, 0, 0);
      foreach (Color pixel in pixels)
      {
        result.R += pixel.R / pixels.Length;
        result.G += pixel.G / pixels.Length;
        result.B += pixel.B / pixels.Length;
      }

      return Color.FromArgb(origA, result.R, result.G, result.B);
    }
  }

  public class StegoInterpolationV2 : StegoInterpolation, IStegoAlgorithm
  {
    static private float diffBoundary = 0.6f; 
    static public int calcInjectedBit(int bitInjectionCount, int newInterValue, int fileByte)
    {
      if (bitInjectionCount > 0)
      {
        int injectedBits = (fileByte & (~(~0 << (bitInjectionCount))));
        return newInterValue + injectedBits > 255 ? Math.Abs(newInterValue - injectedBits) : newInterValue + injectedBits;

      }
      return fileByte;
    }
    static public int Encode(string stegoImagePath, string coveredFilePath)
    {
      ImageBuilder imageBuilder = new ImageBuilder(stegoImagePath);
      FileBuilder fileBuilder = new FileBuilder(coveredFilePath);

      Bitmap imageBitmap = new Bitmap(imageBuilder.Bitmap);
      int totalPixels = imageBitmap.Width * imageBitmap.Height;
      int fileSize = fileBuilder.Data.Count;
      FileIterator fileIterator = new FileIterator(fileBuilder);
      int x = 0;
      int y = 0;
      // if (fileSize * 8 > totalPixels / 3)
      // {
      //   throw new Exception(String.Format("Не возможно внедрить файл в изображение, не хватает {0} бит", Math.Abs(fileBuilder.Data.Count * 8 - totalPixels * 3)));
      // }

      byte fileByte = fileIterator.current;

      while (!fileIterator.isEnd)
      {
        if (y >= imageBitmap.Height)
        {
          x++;
          y = 0;
        }

        if (x >= imageBitmap.Width) {
          Console.WriteLine(String.Format("Ошибка недостаточно пикселей для внедрения! Необходимо внедрить еще: {0} байт", fileIterator.count));
          break;
        }

        Color origPixel = imageBitmap.GetPixel(x, y);
        Color nextNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
        Color prevNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
        Color newInterPixel = Color.FromArgb(0, 0, 0, 0);

        //Если сверху нет соседнего пикселя
        if (y - 1 < 0 && y + 1 < imageBitmap.Height)
        {
          nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
          if (x + 1 < imageBitmap.Width)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel);
          }

          if (x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), nextNeighbourPixel);
          }

          if (x + 1 < imageBitmap.Width && x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel);
          }

        }

        //Если снизу нет соседнего пикселя
        if (y - 1 > 0 && y + 1 >= imageBitmap.Height)
        {
          prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
          if (x + 1 < imageBitmap.Width)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x + 1, y), prevNeighbourPixel);
          }

          if (x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), prevNeighbourPixel);
          }

          if (x + 1 < imageBitmap.Width && x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), prevNeighbourPixel);
          }
        }

        //Если есть сосед сверху и снизу
        if (y - 1 >= 0 && y + 1 < imageBitmap.Height)
        {
          nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
          prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);

          if (x + 1 < imageBitmap.Width)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel, prevNeighbourPixel);
          }

          if (x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), nextNeighbourPixel, prevNeighbourPixel);
          }

          if (x + 1 < imageBitmap.Width && x - 1 > 0)
          {
            newInterPixel = Interpolate(origPixel.A, imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel, prevNeighbourPixel);
          }

        }

        // Выбираем опороный элемент
        Color refPixel = prevNeighbourPixel.Name != "0" ? prevNeighbourPixel : nextNeighbourPixel;
        //  Узнаем разницу между найденным значением и соседним пикселем
        Color diff = Color.FromArgb(
          origPixel.A,
          Math.Abs(newInterPixel.R - refPixel.R),
          Math.Abs(newInterPixel.G - refPixel.G),
          Math.Abs(newInterPixel.B - refPixel.B)
        );

        //  Узнаем разницу между найденным значением и исход
        (float R, float G, float B) diffOrig = (
          (float)Math.Abs(newInterPixel.R - origPixel.R) / (float)origPixel.R,
          (float)Math.Abs(newInterPixel.G - origPixel.G) / (float)origPixel.G,
          (float)Math.Abs(newInterPixel.B - origPixel.B) / (float)origPixel.B
        );

        //  Узнаем сколько бит мы можем скрыть
        Color bitInjectionCount = Color.FromArgb(
          0,
          (int)Math.Round(diff.R > 0 ? Math.Log2(diff.R) : 0),
          (int)Math.Round(diff.G > 0 ? Math.Log2(diff.G) : 0),
          (int)Math.Round(diff.B > 0 ? Math.Log2(diff.B) : 0)
        );

        (int R, int G, int B) newInjectedValue = (0, 0, 0);
        int bitInjectedCount = 0;

        newInjectedValue.R = calcInjectedBit(bitInjectionCount.R, newInterPixel.R, fileByte);
        
        float diffR = (float)Math.Abs(newInjectedValue.R - origPixel.R) / (float)origPixel.R;
        
        if(diffR < diffBoundary) {
          fileByte = (byte)(fileByte >> bitInjectionCount.R);
          bitInjectedCount += bitInjectionCount.R;
        } else {
          newInjectedValue.R = origPixel.R;
        }

        newInjectedValue.G = calcInjectedBit(bitInjectionCount.G, newInterPixel.G, fileByte);
        
        float diffG = (float)Math.Abs(newInjectedValue.G - origPixel.G) / (float)origPixel.G;
        
        if(diffG < diffBoundary) {
          fileByte = (byte)(fileByte >> bitInjectionCount.G);
          bitInjectedCount += bitInjectionCount.G;
        } else {
          newInjectedValue.G = origPixel.G;
        }

        newInjectedValue.B = calcInjectedBit(bitInjectionCount.B, newInterPixel.B, fileByte);
        
        float diffB =  (float)Math.Abs(newInjectedValue.B - origPixel.B) / (float)origPixel.B;
        
        if(diffB < diffBoundary) {
          fileByte = (byte)(fileByte >> bitInjectionCount.B);
          bitInjectedCount += bitInjectionCount.B;
        } else {
          newInjectedValue.B = origPixel.B;
        }

        Color resultPixel = Color.FromArgb(origPixel.A, newInjectedValue.R, newInjectedValue.G, newInjectedValue.B);
        // float coef = 0.1f;
        // int testR = (int)(origPixel.R + origPixel.R * coef) > 255 ? (int)(origPixel.R - origPixel.R * coef) : (int)(origPixel.R + origPixel.R * coef);
        // int testG = (int)(origPixel.G + origPixel.G * coef) > 255 ? (int)(origPixel.G - origPixel.G * coef) : (int)(origPixel.G + origPixel.G * coef);
        // int testB = (int)(origPixel.B + origPixel.B * coef) > 255 ? (int)(origPixel.B - origPixel.B * coef) : (int)(origPixel.B + origPixel.B * coef);


        imageBitmap.SetPixel(x, y, resultPixel);
        //  Внедряем
        // int injectedBits = (diff & (~0 << (bitInjectionCount))) | (fileByte & (~(~0 << (bitInjectionCount))));
        // Если различие больше 0, тогда можно внедрить информацию
        if (bitInjectedCount > 8)
          fileByte = fileIterator.goNext();
        y += 1;
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

            //  Узнаем разницу между найденным значением и исходным
            int diff = Math.Abs(newInterValue - originalPixel.G);
            //  Узнаем сколько бит было скрыто
            int bitInjectionCount = diff > 0 ? (int)Math.Round(Math.Log2(Math.Abs(diff))) : 0;
            // Если различие больше 0, то тогда можно извлечь
            if (bitInjectionCount > 0)
            {
              int injectedBits = (fileByte & (~(~0 << (bitInjectionCount))));
              fileByte |= (byte)(diff << i);
              i += bitInjectionCount;
            }
            y += 2;
          }

          fileBuilder.Data.Add(fileByte);
        }
      }

      fileBuilder.Save(OUT_PATH_DECODE);
    }
  }


  public class StegoInterpolationV1 : StegoInterpolation, IStegoAlgorithm
  {

    static public int Encode(string stegoImagePath, string coveredFilePath)
    {
      ImageBuilder imageBuilder = new ImageBuilder(stegoImagePath);
      FileBuilder fileBuilder = new FileBuilder(coveredFilePath);

      Bitmap imageBitmap = new Bitmap(imageBuilder.Bitmap);
      int totalPixels = imageBitmap.Width * imageBitmap.Height;
      int fileSize = fileBuilder.Data.Count;

      if (fileSize * 8 > totalPixels / 3)
      {
        throw new Exception(String.Format("Не возможно внедрить файл в изображение, не хватает {0} бит", fileBuilder.Data.Count * 8 - totalPixels * 3));
      }

      int fileIterIndex = 0;
      for (int x = 0; x < imageBitmap.Width; x++)
      {
        if (fileIterIndex >= fileSize)
          break;


        for (int y = 0; y < imageBitmap.Height; y++)
        {
          if (fileIterIndex >= fileSize)
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
            Color computePixel = Color.FromArgb(pixel.A, pixel.R, pixel.G, 0);
            Color nextNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            Color prevNeighbourPixel = Color.FromArgb(0, 0, 0, 0);
            int newInterValue = 0;

            //Если сверху нет соседнего пикселя
            if (y - 1 < 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              if (x + 1 < imageBitmap.Width)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel));
              }

              if (x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), nextNeighbourPixel));
              }

              if (x + 1 < imageBitmap.Width && x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel));
              }

            }

            //Если снизу нет соседнего пикселя
            if (y - 1 > 0 && y + 1 >= imageBitmap.Height)
            {
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);
              if (x + 1 < imageBitmap.Width)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x + 1, y), prevNeighbourPixel));
              }

              if (x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), prevNeighbourPixel));
              }

              if (x + 1 < imageBitmap.Width && x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), prevNeighbourPixel));
              }
            }

            //Если есть сосед сверху и снизу
            if (y - 1 > 0 && y + 1 < imageBitmap.Height)
            {
              nextNeighbourPixel = imageBitmap.GetPixel(x, y + 1);
              prevNeighbourPixel = imageBitmap.GetPixel(x, y - 1);

              if (x + 1 < imageBitmap.Width)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel, prevNeighbourPixel));
              }

              if (x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), nextNeighbourPixel, prevNeighbourPixel));
              }

              if (x + 1 < imageBitmap.Width && x - 1 > 0)
              {
                newInterValue = Interpolate(GetByteArrayFromPixels(imageBitmap.GetPixel(x - 1, y), imageBitmap.GetPixel(x + 1, y), nextNeighbourPixel, prevNeighbourPixel));
              }

            }

            if (newInterValue + bit > 255)
            {
              i--;
            }
            else
            {
              resultPixel = Color.FromArgb(pixel.A, pixel.R, newInterValue + bit, pixel.B);
              imageBitmap.SetPixel(x, y, resultPixel);
            }

            y += 1;
          }
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
            y += 1;
          }

          fileBuilder.Data.Add(fileByte);
        }
      }

      fileBuilder.Save(OUT_PATH_DECODE);
    }
  }
}
