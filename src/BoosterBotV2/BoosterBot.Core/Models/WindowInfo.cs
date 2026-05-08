namespace BoosterBot.Core.Models;

public record WindowInfo(
    IntPtr Handle,
    string ProcessName,
    Rect Bounds,
    double ScalingFactor
);
