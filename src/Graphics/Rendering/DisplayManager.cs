namespace ZxenLib.Graphics.Rendering;

using System;
using Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class DisplayManager
{
    private readonly Camera2D mainCamera;
    private readonly GameWindow window;
    private readonly GraphicsConfiguration configuration;
    private readonly GraphicsDeviceManager graphicsManager;
    private readonly SamplerState samplerState = SamplerState.PointClamp;
    private readonly SpriteBatch sb;

    private bool dirty;
    private float virtualAspectRatio = 0;
    private float scaleFactorX = 0;
    private float scaleFactorY = 0;
    private Viewport viewport;
    private RenderTarget2D? virtualScreen;
    private Rectangle screenBounds = Rectangle.Empty;
    private Matrix scaleMatrix = Matrix.Identity;
    private Rectangle virtualScreenBounds = Rectangle.Empty;

    public DisplayManager(GraphicsDeviceManager manager, SpriteBatch sb, IConfigurationManager configManager, GameWindow window)
    {
        this.configuration = configManager.Config.Graphics;
        this.graphicsManager = manager;
        this.sb = sb;
        this.window = window;
        this.mainCamera = new Camera2D(this, true);
        this.ApplySettings();
    }

    /// <summary>
    /// Gets the current bounds of the virtual screen/render target.
    /// </summary>
    public Rectangle VirtualScreenBounds
    {
        get
        {
            if (this.virtualScreenBounds == Rectangle.Empty)
            {
                this.virtualScreenBounds =
                    new Rectangle(
                        0,
                        0,
                        this.VirtualResolutionX,
                        this.VirtualResolutionY);
            }

            return this.virtualScreenBounds;
        }
    }

    /// <summary>
    /// Gets the current bounds of the screen.
    /// </summary>
    public Rectangle ScreenBounds
    {
        get
        {
            if (this.screenBounds == Rectangle.Empty)
            {
                this.screenBounds =
                    new Rectangle(
                        0,
                        0,
                        this.ResolutionX,
                        this.ResolutionY);
            }

            return this.screenBounds;
        }
    }

    /// <summary>
    /// The X/width resolution applied to the <see cref="GraphicsDeviceManager.PreferredBackBufferWidth"/>
    /// </summary>
    public int ResolutionX
    {
        get => this.configuration.ResolutionX;
        private set
        {
            if (value == this.configuration.ResolutionX)
            {
                return;
            }

            this.configuration.ResolutionX = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// The Y/height resolution applied to the <see cref="GraphicsDeviceManager.PreferredBackBufferHeight"/>.
    /// </summary>
    public int ResolutionY
    {
        get => this.configuration.ResolutionY;
        private set
        {
            if (value == this.configuration.ResolutionY)
            {
                return;
            }

            this.configuration.ResolutionY = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// The X/width resolution of the render target.
    /// </summary>
    public int VirtualResolutionX
    {
        get => this.configuration.VirtualResolutionX;
        private set
        {
            if (value == this.configuration.VirtualResolutionX)
            {
                return;
            }

            this.configuration.VirtualResolutionX = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// The Y/height resolution of the render target.
    /// </summary>
    public int VirtualResolutionY
    {
        get => this.configuration.VirtualResolutionY;
        private set
        {
            if (value == this.configuration.VirtualResolutionY)
            {
                return;
            }

            this.configuration.VirtualResolutionY = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Flag indicating if the configuration is set to fullscreen.
    /// </summary>
    public bool IsFullscreen
    {
        get => this.configuration.IsFullScreen;
        set
        {
            if (this.configuration.IsFullScreen == value)
            {
                return;
            }

            this.configuration.IsFullScreen = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Flag indicating if the virtual render target should be upscaled to the native resolution<br/>
    /// or if it should simply render at the native resolution.
    /// </summary>
    public bool UpscaleRenderTarget
    {
        get => this.configuration.UpscaleRenderTarget;
        set
        {
            if (this.configuration.UpscaleRenderTarget == value)
            {
                return;
            }

            this.configuration.UpscaleRenderTarget = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Flag indicating if the screen x-axis should be scaled with real resolution. (PxHOR+)
    /// </summary>
    public bool ScaleX
    {
        get => this.configuration.ScaleX;
        set
        {
            if (this.configuration.ScaleX == value)
            {
                return;
            }

            this.configuration.ScaleX = value;
            this.dirty = true;
        }
    }

    /// <summary>
    /// Sets the real screen Width/Height, which is applied to the preferred backbuffer.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetResolution(int width, int height)
    {
        this.ResolutionX = width;
        this.ResolutionY = height;
        this.ApplySettings();
    }

    /// <summary>
    /// Gets the current RenderTarget. If the render target resolution doesn't match our current<br/>
    /// configuration, build a new one, set it, and return.
    /// </summary>
    /// <returns><see cref="RenderTarget2D"/></returns>
    public RenderTarget2D GetVirtualScreen()
    {
        if (this.virtualScreen == null
            || this.virtualScreen.Width != this.VirtualResolutionX
            || this.virtualScreen.Height != this.VirtualResolutionY)
        {
            this.RebuildRenderTarget();
        }

        return this.virtualScreen;
    }

    /// <summary>
    /// Gets the Resolution Scale matrix between virtual and real resolution.
    /// </summary>
    public Matrix GetScaleMatrix()
    {
        if (!this.UpscaleRenderTarget)
        {
            return Matrix.Identity;
        }

        if (this.scaleMatrix == Matrix.Identity)
        {
            this.CalculateScaleMatrix();
        }

        return this.scaleMatrix;
    }

    /// <summary>
    /// Recalculates the screen bounds.
    /// </summary>
    public void RecalculateScreenBounds()
    {
        // Recalculate virtual screen bounds
        this.virtualScreenBounds =
            new Rectangle(
                0,
                0,
                this.VirtualResolutionX,
                this.VirtualResolutionY);

        // Recalculate actual screen bounds
        this.screenBounds =
            new Rectangle(
                0,
                0,
                this.ResolutionX,
                this.ResolutionY);

        // Recalculate scale matrix.
        this.CalculateScaleMatrix();
    }

    private void ApplySettings()
    {
        Console.WriteLine(this.sb.GetHashCode());

        if (this.IsFullscreen)
        {
            this.ResolutionX = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            this.ResolutionY = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            this.graphicsManager.IsFullScreen = this.IsFullscreen;
        }
        else
        {
            this.graphicsManager.IsFullScreen = this.IsFullscreen;
            this.window.IsBorderless = this.configuration.IsBorderless;
        }

        this.graphicsManager.PreferredBackBufferWidth = this.ResolutionX;
        this.graphicsManager.PreferredBackBufferHeight = this.ResolutionY;

        this.graphicsManager.ApplyChanges();
        this.dirty = true;
    }

    public void ClearBackbuffer()
    {
        this.Clean();
        this.graphicsManager.GraphicsDevice.SetRenderTarget(this.GetVirtualScreen());
        this.graphicsManager.GraphicsDevice.Clear(Color.Black);
    }

    public void BeginEntityDraw()
    {
        this.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera2D.Main.GetCurrentTransformMatrix());
    }

    public void EndEntityDraw()
    {
        this.sb.End();
    }

    public void BeginUiDraw()
    {
        this.sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
    }

    public void EndUiDraw()
    {
        this.sb.End();
    }

    public void Render()
    {
        this.graphicsManager.GraphicsDevice.SetRenderTarget(null);

        this.graphicsManager.GraphicsDevice.Clear(Color.CornflowerBlue);
        this.sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp); // Use PointClamp for crisp pixel art scaling
        this.sb.Draw(this.GetVirtualScreen(), this.ScreenBounds, Color.White);
        this.sb.End();
    }

    private void Clean()
    {
        if (!this.dirty)
        {
            return;
        }

        // letter boxing or pillar boxing
        if (this.configuration.ScreenBoxing)
        {
            // create viewport at aspect target aspect
            float targetAspectRatio = (float)this.ResolutionX / this.ResolutionY;

            // Try width first with scaled height to target aspect (letter box)
            int width = this.ResolutionX;
            int height = (int)(width / targetAspectRatio + 0.5f);

            // Height too big, use screen height and squish width to target aspect (Pillar Box)
            if (height > this.ResolutionY)
            {
                height = this.ResolutionY;
                width = (int)(height * targetAspectRatio + 0.5f);
            }

            // Create Viewport at target aspect ratio, centered in screen.
            this.viewport = new Viewport();
            this.viewport.X = (int)((this.ResolutionX * 0.5f) - (width * 0.5f));
            this.viewport.Y = (int)((this.ResolutionY * 0.5f) - (height * 0.5f));
            this.viewport.Width = width;
            this.viewport.Height = height;
            this.viewport.MinDepth = 0;
            this.viewport.MaxDepth = 1;
        }

        // find closest integer target
        if(!this.UpscaleRenderTarget)
        {
            this.VirtualResolutionX  = this.ResolutionX;
            this.VirtualResolutionY =  this.ResolutionY;
            this.viewport = new Viewport()
            {
                X = 0,
                Y = 0,
                Width = this.ResolutionX,
                Height = this.ResolutionY,
                Bounds = this.screenBounds,
                MaxDepth = 1,
                MinDepth = 0,
            };

            this.RecalculateScreenBounds();
            this.RebuildRenderTarget();
            this.CalculateScaleMatrix();

            this.dirty = false;
            return;
        }

        this.RecalculateScreenBounds();
        this.RebuildRenderTarget();
        this.CalculateScaleMatrix();

        this.dirty = false;
    }

    private void CalculateScaleMatrix()
    {
        this.virtualAspectRatio = (float)this.VirtualResolutionX / this.VirtualResolutionY; // Aspect ratio of the virtual resolution
        int targetWidth, targetHeight;

        // Calculate the target dimensions maintaining the aspect ratio
        if (this.ResolutionX / (float)this.ResolutionY > this.virtualAspectRatio)
        {
            targetHeight = this.ResolutionY;
            targetWidth = (int)(targetHeight * this.virtualAspectRatio);
        }
        else
        {
            targetWidth = this.ResolutionX;
            targetHeight = (int)(targetWidth / this.virtualAspectRatio);
        }

        // Calculate the scaling factors for width and height
        this.scaleFactorX = targetWidth / (float)this.VirtualResolutionX;
        this.scaleFactorY = targetHeight / (float)this.VirtualResolutionY;

        // Create the scale matrix using the calculated scaling factors
        this.scaleMatrix = Matrix.CreateScale(this.scaleFactorX, this.scaleFactorY, 1.0f);
    }

    private void RebuildRenderTarget()
    {
        this.virtualScreen?.Dispose();
        this.virtualScreen = new RenderTarget2D(this.graphicsManager.GraphicsDevice, this.VirtualResolutionX, this.VirtualResolutionY);
        this.viewport = new Viewport(0, 0, this.VirtualResolutionX, this.VirtualResolutionY);
        this.mainCamera.SetCameraViewport(this.viewport);
    }
}
