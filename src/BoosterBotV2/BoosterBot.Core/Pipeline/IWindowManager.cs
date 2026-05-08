using BoosterBot.Core.Models;

namespace BoosterBot.Core.Pipeline;

public interface IWindowManager
{
    WindowInfo? FindGameWindow();
    void FocusWindow(WindowInfo window);
    bool IsWindowVisible(WindowInfo window);
}
