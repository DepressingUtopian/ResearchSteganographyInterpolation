using System;
using System.IO;
using System.Collections;

namespace FileWorker
{
  public class FileBuilder
  {
    private ArrayList data; 
    private string filename;
    public ArrayList Data {
      get {
        return data;
      }
      set {
        data = value;
      }
    }

    public string Filename {
      get {
        return filename;
      }
      set {
        filename = value;
      }
    }

    public FileBuilder() {
      Data = new ArrayList();
    }
    public FileBuilder(string filepath) {
      filename = Path.GetFileName(filepath);
      Open(filepath);
    }
    public byte[] Open(string filepath)
    {
      Data = new ArrayList(File.ReadAllBytes(filepath));
      return (byte[])Data.ToArray(typeof(byte));
    }

    public void Save(string outputPath = "")
    {
      Directory.CreateDirectory(outputPath);
      File.WriteAllBytes(String.Format("{0}/{1}", outputPath, filename), (byte[]) Data.ToArray(typeof(byte)));
    }
  }
}
