namespace ZxenLib.Graphics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ZxenLib.Input;

/// <summary>
/// Implemenation of a 2D camera.
/// </summary>
public class Camera2D
{
    private Viewport myviewport;
    private float zoom;

    public Camera2D(int x, int y, int w, int h, bool isMain = false)
    {
        this.myviewport = new Viewport(x, y, w, h);
        this.zoom = 1.0f;
        this.Rotation = 0.0f;
        this.Position = new Vector2(x, y);

        if (isMain)
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

        if (isMain)
        {
            Camera2D.Main = this;
        }

        this.myviewport = viewport;
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
        get
        {
            return this.zoom;
        }

        set
        {
            this.zoom = value;
            if (this.zoom < 0.1f)
            {
                this.zoom = 0.1f;
            }
            else if (this.zoom > 1.0f)
            {
                this.zoom = 1.0f;
            }
        }
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
    /// Gets a newly created instance of the <see cref="Camera2D"/>'s matrix transformation.
    /// </summary>
    /// <returns><see cref="Matrix"/>.</returns>
    public Matrix GetNewTransformation()
    {
        this.Transform =
            Matrix.CreateTranslation(new Vector3(-this.Position.X, -this.Position.Y, 0)) *
            Matrix.CreateRotationZ(this.Rotation) *
            Matrix.CreateScale(new Vector3(this.Zoom, this.Zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(this.myviewport.Width * 0.5f, this.myviewport.Height * 0.5f, 0));
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