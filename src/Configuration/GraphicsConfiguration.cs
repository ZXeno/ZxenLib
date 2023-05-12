namespace ZxenLib.Configuration;

/// <summary>
/// Defines the current resolution configuration settings.
/// </summary>
public class GraphicsConfiguration
{
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
    /// Gets the virtual X resolution.
    /// </summary>
    public int VirtualResolutionX { get; set;  } = 640;

    /// <summary>
    /// Gets the virtual Y resolution.
    /// </summary>
    public int VirtualResolutionY { get; set; } = 400;

    /// <summary>
    /// Flag indicating if the virtual render target should be upscaled to the native resolution<br/>
    /// or if it should simply render at the native resolution.
    /// </summary>
    public bool UpscaleRenderTarget { get; set; } = true;

    /// <summary>
    /// Flag indicating if the screen x-axis should be scaled with real resolution. (PxHOR+)
    /// </summary>
    public bool ScaleX { get; set; } = true;

    public bool ScreenBoxing { get; set; } = false;

    /// <summary>
    /// Flag indicating that sub-pixel calculations for resolution should be permitted.
    /// </summary>
    public bool PermitSubPixelCalculations { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the game is fullscreen. Default value is false.
    /// </summary>
    public bool IsFullScreen { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the game window is borderless. Default value is false.
    /// </summary>
    public bool IsBorderless { get; set; } = false;

    /// <summary>
    /// Flag indicating if VSYNC is enabled. Should probably be disabled in most scenarios.
    /// </summary>
    public bool IsVsyncEnabled { get; set; } = false;

    /// <summary>
    /// Flag indicating if a fixed framerate should be used. Default is true.
    /// </summary>
    public bool IsFixedTimeStep { get; set; } = true;

    /// <summary>
    /// Sets the target framerate if <see cref="GraphicsConfiguration.IsFixedTimeStep"/> is true.
    /// </summary>
    public int TargetFramerate { get; set; } = 60;

    /// <summary>
    /// Flag indicating whether the mouse is visible in the game. Default value is true.
    /// </summary>
    public bool IsMouseVisible { get; set; } = true;
}