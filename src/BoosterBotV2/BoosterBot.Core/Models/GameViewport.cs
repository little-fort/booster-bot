namespace BoosterBot.Core.Models;

public record GameViewport(
    int Left,
    int Top,
    int Width,
    int Height
)
{
    public int Right => Left + Width;
    public int Bottom => Top + Height;
    public int CenterX => Left + Width / 2;
    public int CenterY => Top + Height / 2;
}
