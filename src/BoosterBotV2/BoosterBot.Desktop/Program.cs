using Avalonia;

namespace BoosterBot.Desktop;

class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Developer tooling (reference capture, detection tests, game input) — see DevCommands
        if (args.Length > 0 && args[0] == "dev")
        {
            Environment.ExitCode = DevCommands.Run(args[1..]);
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
