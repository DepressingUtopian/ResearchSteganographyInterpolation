using System;
using System.Drawing;
using System.IO;

namespace ImageWorker
{
  public class ImageBuilder
  {
    private Bitmap bitmap;

    public Bitmap Bitmap
    {
      get { return bitmap; }
      set { bitmap = value; }
    }
    private string filename;
    private static string outputPath = @"C:\Users\Xen\Documents\StegoResearch\Code\results";

    public Bitmap CurrentBitmap
    {
      get
      {
        return bitmap;
      }

      set
      {
        bitmap = value;
      }
    }
    public ImageBuilder(int widht, int height)
    {
      this.bitmap = new Bitmap(widht, height);
    }

    public ImageBuilder(string path)
    {
      filename = Path.GetFileName(path);
      this.bitmap = new Bitmap(path);
    }

    public Bitmap ReadImage(string path)
    {
      this.bitmap = new Bitmap(path);
      return this.bitmap;
    }

    public void Save(string outputPath = "", string outputFilename = "")
    {
      string resultPath = outputPath.Length > 0 ? outputPath : ImageBuilder.outputPath;
      string resultName = outputFilename.Length > 0 ? outputFilename : filename;
      Directory.CreateDirectory(resultPath);
      bitmap.Save(String.Format("{0}/{1}", resultPath, resultName));
    }
  }
}

