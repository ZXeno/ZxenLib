namespace ZxenLib.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Assets;
using Audio;
using Configuration;
using Entities;
using Events;
using GameScreen;
using Graphics;
using Graphics.Rendering;
using Input;
using Physics;

/// <summary>
/// Extension class to add all types to the Dependency Injection container(s).
/// </summary>
public static class DependencyInjectionBootstrapper
{
    /// <summary>
    /// Adds the various components of ZxenLib library to the built-in <see cref="DependencyContainer"/> of ZxenLib.<br/>
    /// This can be called either before creation of your <see cref="Game"/> class, or from the Initialization method.<br/>
    /// </summary>
    /// <param name="collection">The ZxenLib <see cref="DependencyContainer"/></param>
    public static void AddToZxenLibDiContainer(this DependencyContainer collection)
    {
        collection.Register<IAssetManager, AssetManager>();
        collection.Register<IAudioManager, AudioManager>();
        collection.Register<IConfigurationManager, ConfigurationManager>();
        collection.Register<DisplayManager>();
        collection.Register<IEventDispatcher, EventDispatcher>();
        collection.Register<IEntityManager, EntityManager>();
        collection.Register<FrameDataUtility>();
        collection.Register<GameScreenManager>();
        collection.Register<GameStrings>();
        collection.Register<IInputProvider, InputProvider>();
        collection.Register<SpriteBatch>();
        collection.Register<ISpriteManager, SpriteManager>();
        collection.Register<ZxPhysics2D>();
        collection.Register(collection);
    }
}