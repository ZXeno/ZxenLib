namespace ZxenLib;

using GameScreen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Configuration;
using DependencyInjection;
using Graphics.Rendering;
using Input;
using Physics;

public class ZxGame : Game
{
    protected IAssetManager AssetManager;
    protected IInputProvider Input;
    protected IConfigurationManager ConfigManager;
    protected GameScreenManager GameScreenManager;
    protected GraphicsDeviceManager Graphics;
    protected DependencyContainer ServiceContainer;
    protected DisplayManager DisplayManager;
    protected SpriteBatch SpriteBatch;
    protected ZxPhysics2D physics;

    public ZxGame(
        IAssetManager assetManager,
        IInputProvider inputProvider,
        IConfigurationManager configManager,
        GameScreenManager screenManager,
        ZxPhysics2D physics,
        DependencyContainer serviceContainer)
    {
        this.AssetManager = assetManager;
        this.Input = inputProvider;
        this.GameScreenManager = screenManager;
        this.ConfigManager = configManager;
        this.physics = physics;
        this.ServiceContainer = serviceContainer;

        this.Graphics = new GraphicsDeviceManager(this);
        this.Graphics.GraphicsProfile = GraphicsProfile.Reach;

        this.ServiceContainer.Register(this.Graphics);
        this.ServiceContainer.Register(this.Window);
    }

    protected override void Initialize()
    {
        // The graphics device is called after "Game.Run()" is called. So we register it here.
        this.ServiceContainer.Register(this.Graphics.GraphicsDevice);
        this.AssetManager.Initialize(this);
        this.GameScreenManager.Initialize();
        this.ConfigManager.LoadConfiguration().ConfigureAwait(true);

        this.Init();

        base.Initialize();
    }

    protected virtual void Init()
    {
    }

    protected virtual void BeginLoadContent()
    {
        this.AssetManager.LoadAssets();
        // display manager has a spritebatch dependency, and so must be loaded here.
        this.SpriteBatch = this.ServiceContainer.Resolve<SpriteBatch>();
        this.DisplayManager = this.ServiceContainer.Resolve<DisplayManager>();
    }

    protected override void UnloadContent()
    {
        this.AssetManager.UnloadAssets();
    }

    protected override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        this.Input.Update(deltaTime, this.IsActive);
        this.GameScreenManager.UpdateStates(deltaTime);

        this.Update(deltaTime);
        this.OnLateUpdate(deltaTime);
        this.LateUpdate(deltaTime);
    }

    protected virtual void Update(float deltaTime){}

    protected virtual void OnLateUpdate(float deltaTime) { }

    private void LateUpdate(float deltaTime)
    {
        this.physics.Update(deltaTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        this.OnBeginDraw();

        this.DrawGame();

        this.OnDrawEnd();

        base.Draw(gameTime);
    }

    protected virtual void OnBeginDraw()
    {
        this.DisplayManager.ClearBackbuffer();
    }

    protected virtual void DrawGame() { }

    protected virtual void OnDrawEnd()
    {
        this.DisplayManager.Render();
    }
}