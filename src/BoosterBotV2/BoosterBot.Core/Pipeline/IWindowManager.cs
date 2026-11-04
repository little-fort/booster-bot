using BoosterBot.Core.Models;

namespace BoosterBot.Core.Pipeline;

public interface IWindowManager
{
    WindowInfo? FindGameWindow();
    void FocusWindow(WindowInfo window);
    bool IsWindowVisible(WindowInfo window);

    /// <summary>
    /// Resizes the game window so its client area is width x height pixels (windowed mode only).
    /// Returns the resulting client area in screen coordinates.
    /// </summary>
    Rect ResizeClient(int width, int height);
}
