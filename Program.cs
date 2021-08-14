using System;
using ImageWorker;
using StegoAlgorithms;
using StegoAnalysys;

namespace Code
{
    class Program
    {

        static void Main(string[] args)
        {
          try {
            // imageWorker.Save();
            // int stegoSize = LSB.Encode(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\resource\embed\testSample.txt", "test");
            // // LSB.Decode(@"C:\Users\Xen\Documents\StegoResearch\Code\results\lsb\encode\img (1).jpg","testSample.txt", stegoSize);
            // int stegoSize2 = StegoInterpolationV2.Encode(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\resource\embed\testSample.txt");
            // StegoInterpolationV1.Decode(@"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode\test.jpg","testSample.txt", stegoSize2);

            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", "orig.jpg",  numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\lsb\encode\img (1).jpg", "lsb.jpg", numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode\img (1).jpg", "interpol.jpg", numSelectedBit: 1);
           
            // Console.WriteLine(Metrics.PSNR(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode\img (1).jpg"));
            // Console.WriteLine(Metrics.PSNR(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results\lsb\encode\img (1).jpg"));
            

            // Console.WriteLine(Metrics.IF(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg"));
            Console.WriteLine(Metrics.NCC(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode\img (1).jpg"));
            Console.WriteLine(Metrics.Q(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results\interpol\encode\img (1).jpg"));
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_blue2 (1).jpg", isBlue: true, numSelectedBit: 2);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_green2 (1).jpg", isGreen: true, numSelectedBit: 2);
            Console.WriteLine("Завершено!");
          } catch(Exception error) {
            Console.WriteLine(error.Message);
          }
        }
    }
}
