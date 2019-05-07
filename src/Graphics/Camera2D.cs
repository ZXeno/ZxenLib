namespace ZxenLib.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Input;

    /// <summary>
    /// Implemenation of a 2D camera.
    /// </summary>
    public class Camera2D
    {
        private GraphicsDevice graphicsDevice;
        private float zoom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera2D"/> class.
        /// </summary>
        public Camera2D(GraphicsDevice graphicsDevice)
        {
            this.zoom = 1.0f;
            this.Rotation = 0.0f;
            this.Position = Vector2.Zero;
            Camera2D.Main = this;

            this.graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Gets the main camera instance for all cameras.
        /// </summary>
        public static Camera2D Main { get; private set; }

        /// <summary>
        /// Gets or sets the zoom for this camera. Clamped between 0.1f and 1.0f
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
        /// <returns><see cref="Matrix"/></returns>
        public Matrix GetNewTransformation()
        {
            this.Transform =
              Matrix.CreateTranslation(new Vector3(-this.Position.X, -this.Position.Y, 0)) *
                                         Matrix.CreateRotationZ(this.Rotation) *
                                         Matrix.CreateScale(new Vector3(this.Zoom, this.Zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(this.graphicsDevice.Viewport.Width * 0.5f, this.graphicsDevice.Viewport.Height * 0.5f, 0));
            return this.Transform;
        }

        /// <summary>
        /// Return the 2D world point of the mouse.
        /// </summary>
        /// <param name="viewMatrix">The viewmatrix of the camera view.</param>
        /// <returns><see cref="Vector2"/></returns>
        public Vector2 ScreenToWorldPoint(Matrix viewMatrix)
        {
            Vector2 worldPoint = default(Vector2);

            Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
            worldPoint = Vector2.Transform(InputWrapper.MousePosition.ToVector2(), inverseViewMatrix);

            return worldPoint;
        }

        /// <summary>
        /// Return the 2D world point of the specified point.
        /// </summary>
        /// <param name="position">The position being queried.</param>
        /// <param name="viewMatrix">The viewmatrix of the camera view.</param>
        /// <returns><see cref="Vector2"/></returns>
        public Vector2 ScreenToWorldPoint(Vector2 position, Matrix viewMatrix)
        {
            Vector2 worldPoint = default(Vector2);

            Matrix inverseViewMatrix = Matrix.Invert(viewMatrix);
            worldPoint = Vector2.Transform(position, inverseViewMatrix);

            return worldPoint;
        }
    }
}
