namespace ZxenLib.Graphics;

using Microsoft.Xna.Framework;

/// <summary>
/// Defines the current resolution configuration settings.
/// </summary>
public class ResolutionConfiguration
{
    private Rectangle screenBounds = Rectangle.Empty;

    /// <summary>
    ///  Gets or sets the X resolution configuration option. Default value is 1024.
    /// </summary>
    public int ResolutionX { get; set; } = 1024;

    /// <summary>
    /// Gets or sets the Y resolution configuration option. Default value is 768.
    /// </summary>
    public int ResolutionY { get; set; } = 768;

    /// <summary>
    /// Gets the current bounds of the screen.
    /// </summary>
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
        this.screenBounds =
            new Rectangle(
                0,
                0,
                this.ResolutionX,
                this.ResolutionY);
    }
}