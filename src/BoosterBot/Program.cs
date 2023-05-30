using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace BoosterBot;

internal class Program
{
    static void Main(string[] args)
    {
        bool verbose = false;
        bool autoplay = true;
        bool saveScreens = false;
        double scaling = 1.0;

        // Parse flags:
        if (args.Length > 0)
            for (int i = 0; i < args.Length; i++)
                switch (args[i])
                {
                    case "-scaling":
                    case "--scaling":
                    case "-s":
                        scaling = double.Parse(args[i++]);
                        break;
                    case "-verbose":
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                    case "-quiet":
                    case "--quiet":
                    case "-q":
                        verbose = false;
                        break;
                    case "-noautoplay":
                    case "--noautoplay":
                    case "-na":
                        autoplay = false;
                        break;
                    case "-savescreens":
                    case "--savescreens":
                    case "-ss":
                        saveScreens = true;
                        break;
                }

        try
        {
            var bot = new BoosterBot(scaling, verbose, autoplay, saveScreens);
            bot.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine();
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine("\nPress [Enter] to continue...");
        Console.ReadLine();
    }
}