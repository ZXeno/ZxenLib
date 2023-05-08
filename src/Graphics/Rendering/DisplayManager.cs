namespace ZxenLib.Graphics.Rendering;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class DisplayManager
{
    private Viewport                viewport;
    private GraphicsDeviceManager   graphicsManager;
    private RenderTarget2D?          virtualTarget;
    private int                     screenWidth;
    private int                     screenHeight;
    private int                     referenceWidth;
    private int                     referenceHeight;
    private int                     virtualWidth;
    private int                     virtualHeight;
    private bool                    isFullscreen;
    private bool                    upscaleTarget;
    private bool                    dirty = false;
    private SpriteBatch sb;

    public DisplayManager(GraphicsDeviceManager manager, int width, int height, int refWidth, int refHeight, bool Upscale, bool fullScreen)
    {
        this.graphicsManager   = manager;
        this.screenWidth       = width;
        this.screenHeight      = height;
        this.referenceWidth    = refWidth;
        this.referenceHeight   = refHeight;
        this.isFullscreen      = fullScreen;
        this.upscaleTarget     = Upscale;
        this.ApplySettings();
    }

    public void SetResolution(int Width, int Height, bool fullscreen)
    {
        this.screenWidth = Width;
        this.screenHeight = Height;
        this.isFullscreen = fullscreen;
        this.ApplySettings();
        this.dirty = true;
    }

    public void SetReferenceResolution(int Width, int Height)
    {
        this.referenceWidth    = Width;
        this.referenceHeight   = Height;
        this.dirty = true;
    }

    public void UpscaleTarget(bool value)
    {
        this.upscaleTarget = value;
        this.dirty = true;
    }

    public bool IsUpscaledReference()
    {
        return this.upscaleTarget;
    }

    public int Width
    {
        get { return this.screenWidth; }
    }

    public int Height
    {
        get { return this.screenHeight; }
    }

    public int ReferenceWidth
    {
        get { return this.referenceWidth; }
    }

    public int ReferenceHeight
    {
        get { return this.referenceHeight; }
    }

    public int VirtualWidth
    {
        get { return this.virtualWidth; }
    }

    public int VirtualHeight
    {
        get { return this.virtualHeight; }
    }

    public RenderTarget2D GetVirtualTarget()
    {
        // Recreate the virtual target if its different too our current size.
        if (this.virtualTarget == null
            || this.virtualTarget.Width != this.virtualWidth
            || this.virtualTarget.Height != this.virtualHeight)
        {
            this.virtualTarget = new RenderTarget2D(this.graphicsManager.GraphicsDevice, this.virtualWidth, this.virtualHeight);
        }

        return this.virtualTarget;
    }

    public Matrix GetMatrix()
    {
        // Only scale
        if (!this.upscaleTarget)
        {
            return Matrix.CreateScale((float)this.virtualWidth / this.referenceWidth,
                                      (float)this.virtualHeight / this.referenceHeight, 1f);
        }

        // Its not efficient but works
        return Matrix.Identity;
    }

    public bool IsFullscreen()
    {
        return this.isFullscreen;
    }

    private void ApplySettings()
    {
        if(!this.isFullscreen)
        {
            if (this.screenWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width
                && this.screenHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height)
            {
                this.graphicsManager.PreferredBackBufferWidth  = this.screenWidth;
                this.graphicsManager.PreferredBackBufferHeight = this.screenHeight;
                this.graphicsManager.IsFullScreen              = this.isFullscreen;
                this.graphicsManager.ApplyChanges();
            }

            return;
        }

        this.graphicsManager.PreferredBackBufferWidth  = this.screenWidth;
        this.graphicsManager.PreferredBackBufferHeight = this.screenHeight;
        this.graphicsManager.IsFullScreen              = this.isFullscreen;
        this.graphicsManager.ApplyChanges();
    }

    public void BeginDraw()
    {
        // Clear the entire back buffer first!
        this.graphicsManager.GraphicsDevice.Viewport = new Viewport(0, 0, this.screenWidth, this.screenHeight);
        this.graphicsManager.GraphicsDevice.Clear(Color.Black);

        this.UpdateData();
        this.graphicsManager.GraphicsDevice.Viewport = this.viewport;

    }

    private void UpdateData()
    {
        if (this.dirty)
        {
            //--CREATE VIEWPORT AT ASPECT TARGET ASPECT--
            float targetAspectRatio = (float)this.referenceWidth / this.referenceHeight;

            // Try width first with scaled height to target aspect (letter box)
            int width = this.screenWidth;
            int height = (int)(width / targetAspectRatio + 0.5f);

            // Height too big, use screen height and squish width to target aspect (Pillar Box)
            if (height > this.screenHeight)
            {
                height = this.screenHeight;
                width = (int)(height * targetAspectRatio + 0.5f);
            }

            // Create Viewport at target aaspect ratio, centered in screen.
            this.viewport = new Viewport();
            this.viewport.X = (int)((this.screenWidth * 0.5f) - (width * 0.5f));
            this.viewport.Y = (int)((this.screenHeight * 0.5f) - (height * 0.5f));
            this.viewport.Width = width;
            this.viewport.Height = height;
            this.viewport.MinDepth = 0;
            this.viewport.MaxDepth = 1;

            //--FIND CLOSEST INTEGER TARGET--
            if(this.upscaleTarget == false)
            {
                float sw = (float)this.screenWidth / this.referenceWidth;
                float sh = (float)this.screenHeight / this.referenceHeight;
                int integerScaling = 1;

                if(sw > sh)
                {
                    integerScaling = (int)sw;
                }
                else
                {
                    integerScaling = (int)sh;
                }

                // Set virtual too be the closest integer scaling too screen size!
                this.virtualWidth  = this.referenceWidth  * integerScaling;
                this.virtualHeight = this.referenceHeight * integerScaling;
            }
            else
            {
                this.virtualWidth  = this.referenceWidth;
                this.virtualHeight = this.referenceHeight;
            }

            this.dirty = false;
        }
    }

    public void BlitTarget(RenderTarget2D virtualTarget)
    {
        SamplerState samplerState = SamplerState.PointClamp;
        if (this.upscaleTarget == false)
        {
            samplerState = SamplerState.LinearClamp;
        }

        this.sb.Begin(SpriteSortMode.Immediate, null, samplerState);
        this.sb.Draw(virtualTarget, new Rectangle(this.viewport.X, this.viewport.Y, this.viewport.Width, this.viewport.Height), new Rectangle(0,0, this.virtualWidth, this.virtualHeight), Color.White);
        this.sb.End();
    }

}
