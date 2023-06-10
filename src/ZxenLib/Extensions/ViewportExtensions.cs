namespace ZxenLib.Extensions;

using Microsoft.Xna.Framework.Graphics;

public static class ViewportExtensions
{
    public static readonly Viewport ZeroViewport = new Viewport(0, 0, 0, 0);

    public static bool IsZero(this Viewport viewport)
    {
        return viewport.Width == 0 && viewport.Height == 0 && viewport.MaxDepth == 0f;
    }
}