namespace ZxenLib.Graphics;

using System;
using Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rendering;
using ZxenLib.Input;

/// <summary>
/// Implementation of a 2D camera.
/// </summary>
public class Camera2D
{
    private readonly DisplayManager displayManager;

    private float zoom;

    private Viewport myViewport;
    private RenderTarget2D? myRenderTarget;
    private SpriteSortMode spriteSortMode = SpriteSortMode.Deferred;
    private BlendState blendState = BlendState.AlphaBlend;
    private SamplerState samplerState = SamplerState.PointClamp;
    private DepthStencilState? depthStencilState = null;
    private RasterizerState? rasterizerState = null;
    private Effect? effect = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(DisplayManager displayManager, bool isMain = false)
    {
        this.displayManager = displayManager;
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.CameraSortId = Ids.GetNewCameraIndex();
        this.myViewport = new Viewport(
            0, 0,
            displayManager.VirtualResolutionX,
            displayManager.VirtualResolutionY);

        if (isMain)
        {
            Camera2D.Main = this;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(DisplayManager displayManager, int x, int y, int w, int h, bool isMain = false)
    {
        this.displayManager = displayManager;
        this.myViewport = new Viewport(x, y, w, h);
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.CameraSortId = Ids.GetNewCameraIndex();
        this.Position = new Vector2(x, y);

        if (isMain)
        {
            Camera2D.Main = this;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(DisplayManager displayManager, Viewport viewport, Vector2 position = default, bool isMain = true)
    {
        this.displayManager = displayManager;
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.Position = position;
        this.myViewport = viewport;
        this.CameraSortId = Ids.GetNewCameraIndex();

        if (isMain)
        {
            Camera2D.Main = this;
        }
    }

    private SpriteBatch SpriteBatch => this.displayManager.SpriteBatch;

    /// <summary>
    /// Gets the main camera instance for all cameras.
    /// </summary>
    public static Camera2D Main { get; private set; }

    /// <summary>
    /// Determines in what order this camera will have its Render function called by the DisplayManager.
    /// </summary>
    public ushort CameraSortId { get; set; }

    /// <summary>
    /// Gets or sets the zoom for this camera. Clamped between 0.1f and 1.0f.
    /// </summary>
    public float Zoom
    {
        get => this.zoom;
        set => this.zoom = Math.Clamp(value, 0.1f, 1.0f);
    }

    /// <summary>
    /// Gets or sets the camera's transform matrix.
    /// </summary>
    public Matrix Transform { get; set; } // Matrix Transform

    /// <summary>
    /// Gets or sets the camera's rotation.
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Gets or sets the camera's position.
    /// </summary>
    public Vector2 Position { get; set; }

    public SpriteSortMode SpriteSortMode
    {
        get => this.spriteSortMode;
        set => this.spriteSortMode = value;
    }

    public BlendState BlendState
    {
        get => this.blendState;
        set => this.blendState = value;
    }

    public SamplerState SamplerState
    {
        get => this.samplerState;
        set => this.samplerState = value;
    }

    public DepthStencilState? DepthStencilState
    {
        get => this.depthStencilState;
        set => this.depthStencilState = value;
    }

    public RasterizerState? RasterizerState
    {
        get => this.rasterizerState;
        set => this.rasterizerState = value;
    }

    public Effect? Effect
    {
        get => this.effect;
        set => this.effect = value;
    }

    public Color ClearBackbufferColor { get; set; } = Color.Black;

    public bool UseTransformMatrix { get; set; } = true;

    /// <summary>
    /// Sets the camera viewport.
    /// </summary>
    /// <param name="viewport"></param>
    public void SetCameraViewport(Viewport viewport)
    {
        this.myViewport = viewport;
    }

    /// <summary>
    /// Gets a new instance of the <see cref="Camera2D"/>'s current matrix transform.
    /// </summary>
    /// <returns><see cref="Matrix"/></returns>
    public Matrix GetCurrentTransformMatrix(Matrix? scale = null)
    {
        scale ??= Matrix.Identity;
        Matrix cameraZoomMatrix = Matrix.CreateScale(new Vector3(this.Zoom, this.Zoom, 1));

        this.Transform =
            Matrix.CreateTranslation(new Vector3(-this.Position.X, -this.Position.Y, 0)) *
            Matrix.CreateRotationZ(this.Rotation) *
            cameraZoomMatrix *
            scale.Value *
            Matrix.CreateTranslation(new Vector3(this.myViewport.Width * 0.5f, this.myViewport.Height * 0.5f, 0));
        return this.Transform;
    }

    /// <summary>
    /// Return the 2D world point of the mouse.
    /// </summary>
    /// <param name="viewMatrix">The viewmatrix of the camera view.</param>
    /// <returns><see cref="Vector2"/>.</returns>
    public Vector2 MouseScreenToWorldPoint(Matrix viewMatrix)
    {
        Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
        return Vector2.Transform(InputProvider.Mouse.MousePosition.ToVector2(), inverseViewMatrix);
    }

    /// <summary>
    /// Return the 2D world point of the mouse.
    /// </summary>
    /// <param name="viewMatrix">The viewmatrix of the camera view.</param>
    /// <returns><see cref="Vector2"/>.</returns>
    public Vector2 ScreenToWorldPoint(Matrix viewMatrix)
    {
        Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
        return Vector2.Transform(InputProvider.Mouse.MousePosition.ToVector2(), inverseViewMatrix);
    }

    /// <summary>
    /// Return the 2D world point of the specified point.
    /// </summary>
    /// <param name="position">The position being queried.</param>
    /// <param name="viewMatrix">The viewmatrix of the camera view.</param>
    /// <returns><see cref="Vector2"/>.</returns>
    public Vector2 ScreenToWorldPoint(Vector2 position, Matrix viewMatrix)
    {
        Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
        return Vector2.Transform(position, inverseViewMatrix);
    }

    public void ClearBuffer()
    {
        this.displayManager.ClearBackbuffer();
    }

    public void BeginDraw()
    {
        this.displayManager.SetCurrentRenderTarget(this.GetRenderTarget(), this.ClearBackbufferColor);
        this.SpriteBatch.Begin(
            this.spriteSortMode,
            this.blendState,
            this.samplerState,
            this.depthStencilState,
            this.rasterizerState,
            this.effect,
            this.UseTransformMatrix ? this.GetCurrentTransformMatrix() : Matrix.Identity);
    }

    public void EndDraw()
    {
        this.SpriteBatch.End();
    }

    public void Render(SpriteBatch renderBatch)
    {
        renderBatch.Draw(this.myRenderTarget, this.displayManager.ScreenBounds, Color.White);
    }

    private RenderTarget2D GetRenderTarget()
    {
        if (this.myRenderTarget == null
            || this.myRenderTarget.Width != this.myViewport.Width
            || this.myRenderTarget.Height != this.myViewport.Height)
        {
            this.myRenderTarget?.Dispose();
            this.myRenderTarget = this.displayManager.CreateRenderTarget(this.myViewport.Width, this.myViewport.Height);
            this.myViewport = new Viewport(
                0, 0,
                this.displayManager.VirtualResolutionX,
                this.displayManager.VirtualResolutionY);
        }

        return this.myRenderTarget;
    }

    public void Dispose()
    {
        this.myRenderTarget?.Dispose();
    }
}