namespace ZxenLib.Graphics.Rendering;

using Configuration;
using Microsoft.Xna.Framework.Graphics;

public static class RenderTargetFactory
{
    public static RenderTarget2D CreateRenderTarget(ResolutionConfiguration config, GraphicsDevice graphicsDevice)
    {
        RenderTarget2D renderTarget = new (graphicsDevice, config.VirtualResolutionX, config.VirtualResolutionY);

        return renderTarget;
    }
}