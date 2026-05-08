using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BoosterBot.Core.Pipeline;
using BoosterBot.Desktop.ViewModels;
using BoosterBot.Desktop.Views;
using BoosterBot.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace BoosterBot.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddWindowsPlatform();

        var provider = services.BuildServiceProvider();

        var referencePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Reference");
        if (Directory.Exists(referencePath))
            ImageProcessor.LoadReferenceImages(referencePath, "en");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(
                    provider.GetRequiredService<IScreenCapture>(),
                    provider.GetRequiredService<IWindowManager>())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
