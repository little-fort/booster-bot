using OpenCvSharp;

namespace BoosterBot.Core.Models;

public sealed class ScreenCaptureContext : IDisposable
{
    public Mat Screenshot { get; }
    public Dimension Dimensions { get; }
    public int Center { get; }
    public int VCenter { get; }
    public Rect WindowRect { get; }

    public ScreenCaptureContext(Mat screenshot, Dimension dimensions, Rect windowRect)
    {
        Screenshot = screenshot;
        Dimensions = dimensions;
        Center = dimensions.Width / 2;
        VCenter = dimensions.Height / 2;
        WindowRect = windowRect;
    }

    public void Dispose() => Screenshot.Dispose();
}
