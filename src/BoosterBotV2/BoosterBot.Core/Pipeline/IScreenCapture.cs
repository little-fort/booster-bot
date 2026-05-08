using BoosterBot.Core.Models;

namespace BoosterBot.Core.Pipeline;

public interface IScreenCapture
{
    ScreenCaptureContext Capture();
}
