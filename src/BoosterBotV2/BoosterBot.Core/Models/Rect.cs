using System.Runtime.InteropServices;

namespace BoosterBot.Core.Models;

[StructLayout(LayoutKind.Sequential)]
public struct Rect
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}
