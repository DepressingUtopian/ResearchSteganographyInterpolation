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
            int stegoSize = LSB.Encode(@"C:\Users\Xen\Documents\StegoResearch\Code\resource\images\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\resource\embed\testSample.txt", "test");
            //LSB.Decode(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg",@"C:\Users\Xen\Documents\StegoResearch\Code\results\testSample.txt", stegoSize);
            // int stegoSize = StegoInterpolation.Encode(@"C:\Users\Xen\Documents\StegoResearch\Code\ImageSample\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\resource\files\testSample.txt");
            // StegoInterpolation.Decode(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg","testSample.txt", stegoSize);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\ImageSample\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_red_orig (1).jpg", true, isBlue: true, isGreen: true, numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\ImageSample\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_blue_orig (1).jpg", isBlue: true, numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\ImageSample\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_green_orig (1).jpg", isGreen: true, numSelectedBit: 1);
            
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", true, isBlue: true, isGreen: true,  numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_blue (1).jpg", isBlue: true, numSelectedBit: 1);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_green (1).jpg", isGreen: true, numSelectedBit: 1);

            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_red2 (1).jpg", true, numSelectedBit: 2);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_blue2 (1).jpg", isBlue: true, numSelectedBit: 2);
            // VisualAttack.getPixelComponentToFile(@"C:\Users\Xen\Documents\StegoResearch\Code\results\img (1).jpg", @"C:\Users\Xen\Documents\StegoResearch\Code\results_analysys\img_green2 (1).jpg", isGreen: true, numSelectedBit: 2);
            Console.WriteLine("Завершено!");
          } catch(Exception error) {
            Console.WriteLine(error.Message);
          }
        }
    }
}
