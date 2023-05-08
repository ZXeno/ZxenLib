namespace ZxenLib.Configuration;

using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

/// <summary>
/// Defines the current resolution configuration settings.
/// </summary>
public class ResolutionConfiguration
{
    private Rectangle screenBounds = Rectangle.Empty;
    private Rectangle virtualScreenBounds = Rectangle.Empty;
    private Matrix scaleMatrix = Matrix.Identity;

    /// <summary>
    /// Flag indicating that we should maintain the aspect ratio of our render target. This means nothing if we aren't rendering to a render target.
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = false;

    /// <summary>
    ///  Gets or sets the X resolution configuration option. Default value is 1280.
    /// </summary>
    public int ResolutionX { get; set; } = 1280;

    /// <summary>
    /// Gets or sets the Y resolution configuration option. Default value is 720.
    /// </summary>
    public int ResolutionY { get; set; } = 800;

    /// <summary>
    /// Gets the virutal X resolution.
    /// </summary>
    public int VirtualResolutionX { get; } = 640;

    /// <summary>
    /// Gets the virtual Y resolution.
    /// </summary>
    public int VirtualResolutionY { get; } = 400;

    /// <summary>
    /// Gets the current bounds of the virtual screen.
    /// </summary>
    [JsonIgnore]
    public Rectangle VirtualScreenBounds
    {
        get
        {
            if (this.virtualScreenBounds == null || this.virtualScreenBounds == Rectangle.Empty)
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
    [JsonIgnore]
    public Rectangle ScreenBounds
    {
        get
        {
            if (this.screenBounds == null || this.screenBounds == Rectangle.Empty)
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
    /// Gets the Resolution Scale matrix between virtual and actual resolution.
    /// </summary>
    [JsonIgnore]
    public Matrix ResolutionScale
    {
        get
        {
            if (this.scaleMatrix == null || this.scaleMatrix == Matrix.Identity)
            {
                this.CalculateScaleMatrix();
            }

            return this.scaleMatrix;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the game is fullscreen. Default value is false.
    /// </summary>
    public bool IsFullScreen { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the game window is borderless. Default value is false.
    /// </summary>
    public bool IsBorderless { get; set; } = false;

    /// <summary>
    /// Recalculates the screen bounds.
    /// </summary>
    public void RecalculateScreenBounds()
    {
        // Recalculate virutal screen bounds
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

    /// <summary>
    /// Recalculates the scaling matrix.
    /// </summary>
    private void CalculateScaleMatrix()
    {
        float scaleX = (float)this.ResolutionX / this.VirtualResolutionX;
        float scaleY = (float)this.ResolutionY / this.VirtualResolutionY;
        this.scaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1.0f);
    }
}