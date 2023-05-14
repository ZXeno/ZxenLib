namespace ZxenLib.Graphics.Rendering;

using System;
using System.Collections.Generic;
using Configuration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class DisplayManager
{
    private readonly Camera2D mainCamera;
    private readonly IConfigurationManager configManager;
    private readonly GameWindow window;
    private readonly GraphicsConfiguration configuration;
    private readonly GraphicsDeviceManager graphicsManager;
    private readonly List<Camera2D> cameras;
    private readonly SpriteBatch sb;

    private bool dirty;
    private float virtualAspectRatio = 0;
    private float scaleFactorX = 0;
    private float scaleFactorY = 0;
    private Viewport viewport;
    private Rectangle screenBounds = Rectangle.Empty;
    private Matrix scaleMatrix = Matrix.Identity;
    private Rectangle virtualScreenBounds = Rectangle.Empty;

    public DisplayManager(GraphicsDeviceManager manager, SpriteBatch sb, IConfigurationManager configManager, GameWindow window)
    {
        this.configManager = configManager;
        this.configuration = configManager.Config.Graphics;
        this.graphicsManager = manager;
        this.cameras = new List<Camera2D>(32);
        this.mainCamera = new Camera2D(this, true);
        this.cameras.Add(this.mainCamera);
        this.sb = sb;
        this.window = window;
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

    public SpriteBatch SpriteBatch => this.sb;

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
    /// Adds a camera to the list of cameras. If the sort index is less than the last camera, the draw order list will be resorted.
    /// </summary>
    /// <param name="newCamera"></param>
    public void AddCamera(Camera2D newCamera)
    {
        // if (this.cameras.Count == 0)
        // {
        //     this.cameras.Add(newCamera);
        //     return;
        // }

        ushort lastCameraSortIndex = this.cameras[^1].CameraSortId;
        this.cameras.Add(newCamera);
        if (newCamera.CameraSortId < lastCameraSortIndex)
        {
            this.cameras.Sort((a, b) => a.CameraSortId.CompareTo(b.CameraSortId));
        }
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

    /// <summary>
    /// Creates a new render target. Uses the Virtual Resolution if parameters are not provided.
    /// </summary>
    /// <param name="vResX">Width of the virtual screen to use.</param>
    /// <param name="vResY">Height of the virtual screen to use.</param>
    /// <returns></returns>
    public RenderTarget2D CreateRenderTarget(int vResX = -1, int vResY = -1)
    {

        if (vResX <= 0)
        {
            vResX = this.VirtualResolutionX;
        }

        if (vResY <= 0)
        {
            vResY = this.VirtualResolutionY;
        }

        return new RenderTarget2D(this.graphicsManager.GraphicsDevice, vResX, vResY);
    }

    /// <summary>
    /// Clears the <see cref="GraphicsDevice"/> back buffer and removes the current render target.
    /// </summary>
    public void ClearBackbuffer()
    {
        this.Clean();
        this.SetCurrentRenderTarget(null);
    }

    /// <summary>
    /// Sets the <see cref="GraphicsDevice"/> to a new render target and clears the target.
    /// </summary>
    /// <param name="renderTarget"></param>
    public void SetCurrentRenderTarget(RenderTarget2D? renderTarget, Color? clearColor = null)
    {
        clearColor ??= Color.Black;
        this.graphicsManager.GraphicsDevice.SetRenderTarget(renderTarget);
        this.graphicsManager.GraphicsDevice.Clear(clearColor.Value);
    }

    /// <summary>
    /// Clears the buffer, loops through all cameras and draws their render targets to the screen.
    /// </summary>
    public void Render()
    {
        this.graphicsManager.GraphicsDevice.SetRenderTarget(null);
        this.graphicsManager.GraphicsDevice.Clear(Color.CornflowerBlue);

        this.sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp); // Use PointClamp for crisp pixel art scaling
        foreach (Camera2D camera in this.cameras)
        {
            camera.Render(this.sb);
        }
        this.sb.End();
    }

    /// <summary>
    /// If a configuration setting was changed, or the window was resized, we clean those here,<br/>
    /// then mark all cameras as dirty;
    /// </summary>
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
            this.viewport = new Viewport
            {
                X = (int)((this.ResolutionX * 0.5f) - (width * 0.5f)),
                Y = (int)((this.ResolutionY * 0.5f) - (height * 0.5f)),
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1,
            };

            this.mainCamera.SetCameraViewport(this.viewport);
        }

        // find closest integer target
        if(!this.UpscaleRenderTarget)
        {
            this.VirtualResolutionX  = this.ResolutionX;
            this.VirtualResolutionY =  this.ResolutionY;
        }

        this.RecalculateScreenBounds();
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
}
