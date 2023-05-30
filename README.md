# ZxenLib

ZxenLib is a set of opinionated, code-first extensions, add-on, and replacement systems for Monogame that makes it easier to prototype and build 2D games. Currently, this project is an early work-in-progress that is in constant development by a single person.

## Getting Started

(THIS GETTING STARTED SECTION ASSUMES YOU ARE RUNNING THE v0.1.4.0 BRANCH!)

There are plans for a nuget package in the near future, after the `.0.1.4` branch is merged. If you wish to add ZxenLib to your project as of now, you can use a git submodule, or clone the repo locally and use/extend it from there.

To integrate into your project:
First, you will need to alter your `Program.cs` file to create a new `DependencyContainer`, add the ZxenLib classes/systems to the DI container, register your game class, then resolve and instantiate your game class.

Example:

```csharp
namespace MyGameProject;

using Microsoft.Xna.Framework;
using ZxenLib.DependencyInjection;

internal class Program
{
    public static DependencyContainer ServiceContainer { get; private set; }

    public static void Main(string[] args)
    {
        ServiceContainer = new();
        ServiceContainer.AddToZxenLibDiContainer();
        ServiceContainer.Register<Game, Game1>();

        using Game game = ServiceContainer.Resolve<Game>();
        game.Run();
    }
}
```

In your game root class (typically `Game1` by the default template), extend from ZxGame instead of the default `Game` class:

```csharp
public class Game1 : ZxGame
{
    public Game1(
        IAssetManager assetManager,
        IInputProvider inputProvider,
        IConfigurationManager configManager,
        GameScreenManager screenManager,
        DependencyContainer serviceContainer)
        : base(assetManager, inputProvider, configManager, screenManager, serviceContainer)
    {
        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }
}
```

`ZxGame` overrides the `Initialize()` method to handle specifically-timed dependency registration/instantiation for various components. ZxGame will call `Init()` before passing back to Monogame for completing initialization. Place your initialization code in this new `Init()` method.

```csharp
    protected override void Init()
    {
        // --- Apply game class specific settings here ---
        // This is called during ZxGame.Initialization after ZxenLib services have been
        // initialized, but before passing the call back to the main framework initialization process.
        this.IsFixedTimeStep = this.ConfigManager.Config.Graphics.IsFixedTimeStep;
        this.IsMouseVisible = this.ConfigManager.Config.Graphics.IsMouseVisible;
        this.Graphics.SynchronizeWithVerticalRetrace = this.ConfigManager.Config.Graphics.IsVsyncEnabled;

        this.Graphics.ApplyChanges();
    }
}
```

During `LoadContent()` you will need to call `base.BeginLoadContent()` to ensure your assets are loaded, the `SpriteBatch` and `DisplayManager` classes are successfully created.

If you want to use the `FrameDataUtility`, this is also the places to load it.

```csharp
protected override void LoadContent()
{
    // Load ZxenLib content systems here. This call must come first!
    base.BeginLoadContent();

    // This requires a spritefont component, ensure it loads after Asset Manager assets are loaded.
    this.FrameData = this.ServiceContainer.Resolve<FrameDataUtility>();

    // Set the first screen that should appear
    this.GameScreenManager.RegisterScreenType<MyScreen>();
    this.GameScreenManager.ChangeState<MyScreen>();

    // Return to Monogame's LoadContent pipeline
    base.LoadContent();
}
```

If you need to call UnloadContent, be sure to call `base.UnloadContent()` to ensure all asset manager content is unloaded.

```csharp
protected override void UnloadContent()
{
    // This must call the base unload content
    // if you don't call AssetManager.UnloadAssets().
    base.UnloadContent();
}
```

For your update function, be sure to override `Update(float deltaTime)` rather than the base `Update(GameTime gameTime)` method, as this is called as part of the `ZxGame.Update(GameTime gameTime)` method. Other systems are updated before passing on the update call.

```csharp
protected override void Update(float deltaTime)
{
    this.FrameData.Update(deltaTime);
    if (InputProvider.Keyboard.GetKeyDown(Keys.OemTilde))
    {
        this.showDebugInfo = !this.showDebugInfo;
    }
}
```

Finally, for drawing, override the `DrawGame()` method. Ensure you call `GameScreenManager.DrawStates()` first. Any additional draw code can be handled here, such as for debug or a debug camera. Here is an example that includes a debug camera draw pass:

```csharp
protected override void DrawGame()
{
    // Draw the current game sreen.
    this.GameScreenManager.DrawStates();

    // TODO: Draw Debug here
    if (this.showDebugInfo)
    {
        this.DebugCamera.BeginDraw();

        this.FrameData.Draw();
        this.SpriteBatch.DrawString(
            this.AssetManager.FontsDictionary["uifont"],
            $"{this.spinChars[this.counterIndex]}",
            new Vector2(10, 90),
            Color.White);

        this.DebugCamera.EndDraw();
    }


}
```

### Roadmap

The current roadmap for features coming in 0.1.4.0 include:<br/>
[X] ECS Updates<br/>
[ ] Basic Physics System<br/>
[X] More Unit Tests<br/>
[X] Performance Improvements<br/>
[X] Proper Resolution Independence<br/>
[X] First-pass Particle System Implementation<br/>
[ ] At least one example project<br/>
[ ] Nuget Package Release<br/>
