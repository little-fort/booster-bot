using BoosterBot.Core.Input;
using BoosterBot.Core.Pipeline;
using BoosterBot.Platform.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace BoosterBot.Platform;

public static class PlatformServiceRegistration
{
    public static IServiceCollection AddWindowsPlatform(this IServiceCollection services, double scaling = 1.0, string[]? processNames = null)
    {
        var windowManager = new Win32WindowManager(processNames);

        services.AddSingleton(windowManager);
        services.AddSingleton<IWindowManager>(windowManager);
        services.AddSingleton<IScreenCapture>(new Win32ScreenCapture(windowManager, scaling));
        services.AddSingleton<IInputSimulator>(new Win32InputSimulator(windowManager));

        return services;
    }
}
