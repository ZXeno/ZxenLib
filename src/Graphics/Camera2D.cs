namespace ZxenLib.Graphics;

using System;
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

    private Viewport myViewport;
    private float zoom;
    private bool isMain;

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(DisplayManager displayManager, bool isMain = false)
    {
        this.displayManager = displayManager;
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.myViewport = new Viewport(
            0, 0,
            displayManager.VirtualResolutionX,
            displayManager.VirtualResolutionY);

        this.isMain = isMain;
        if (this.isMain)
        {
            Camera2D.Main = this;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(int x, int y, int w, int h, bool isMain = false)
    {
        this.myViewport = new Viewport(x, y, w, h);
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.Position = new Vector2(x, y);
        this.isMain = isMain;

        if (this.isMain)
        {
            Camera2D.Main = this;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera2D"/> class.
    /// </summary>
    public Camera2D(Viewport viewport, Vector2 position = default, bool isMain = true)
    {
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.Position = position;
        this.isMain = isMain;

        if (this.isMain)
        {
            Camera2D.Main = this;
        }

        this.myViewport = viewport;
    }

    /// <summary>
    /// Gets the main camera instance for all cameras.
    /// </summary>
    public static Camera2D Main { get; private set; }

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
}