namespace ZxenLib;

using Assets;
using Infrastructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Audio;
using Configuration;
using Entities;
using Events;
using GameScreen;
using Graphics;
using Input;

/// <summary>
/// Extension class to add all types to the Dependency Injection container(s).
/// </summary>
public static class DependencyInjectionBootstrapper
{
    /// <summary>
    /// Adds the various components of the ZxenLib library to the <see cref="IServiceCollection"/><br/>
    /// of the Microsoft.Extensions.DependencyInjection library.
    /// </summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to register the various types to.</param>
    public static void AddZxenLibToMsDiContainer(this IServiceCollection collection)
    {
        collection.AddSingleton<ConfigurationManager>();
        collection.AddSingleton<GameScreenManager>();
        collection.AddSingleton<IInputProvider, InputProvider>();
        collection.AddSingleton<IEventDispatcher, EventDispatcher>();
        collection.AddSingleton<IEntityManager, EntityManager>();
        collection.AddSingleton<IAssetManager, AssetManager>();
        collection.AddSingleton<ISpriteManager, SpriteManager>();
        collection.AddSingleton<IAudioManager, AudioManager>();
        collection.AddSingleton<GameStrings, GameStrings>();
    }

    /// <summary>
    /// Adds the various components of ZxenLib library to the <see cref="GameServiceContainer"/> of monogame.<br/>
    /// this should be called in the <see cref="Game.Initialize"/> method of your game class. Please note<br/>
    /// that Monogame's container DOES NOT RESOLVE DEPENDENCIES, ONLY GETS INSTANCES. So if you need to have an object<br/>
    /// resolve dependencies as part <see cref="GameServiceContainer.GetService"/>, you will need to handle that<br/>
    /// when it is needed, rather than having the service container handle it.
    /// </summary>
    /// <param name="collection">The monogame <see cref="GameServiceContainer"/>.</param>
    /// <param name="game">The <see cref="Game"/> class dependency.</param>
    public static void AddZxenLibToMonogameServices(this GameServiceContainer collection, Game game)
    {
        EventDispatcher eventDispatcher = new();
        ConfigurationManager cfgManager = new();
        GameScreenManager screenMgr = new(eventDispatcher);
        InputProvider inputProvider = new();
        EntityManager entityManager = new(eventDispatcher);
        SpriteManager spriteManager = new();
        AssetManager assetManager = new(spriteManager);
        AudioManager audioManager = new(eventDispatcher, assetManager);
        GameStrings strings = new(assetManager);

        collection.AddService(cfgManager);
        collection.AddService(screenMgr);
        collection.AddService<IInputProvider>(inputProvider);
        collection.AddService<IEventDispatcher>(eventDispatcher);
        collection.AddService<IEntityManager>(entityManager);
        collection.AddService<IAssetManager>(assetManager);
        collection.AddService<ISpriteManager>(spriteManager);
        collection.AddService<IAudioManager>(audioManager);
        collection.AddService(strings);
    }

    /// <summary>
    /// Adds the various components of ZxenLib library to the built-in <see cref="DependencyContainer"/> of ZxenLib.<br/>
    /// This can be called either before creation of your <see cref="Game"/> class, or from the Initialization method.<br/>
    /// </summary>
    /// <param name="collection">The ZxenLib <see cref="DependencyContainer"/></param>
    public static void AddToZxenLibDiContainer(this DependencyContainer collection)
    {
        collection.Register<ConfigurationManager>();
        collection.Register<GameScreenManager>();
        collection.Register<IInputProvider, InputProvider>();
        collection.Register<IEventDispatcher, EventDispatcher>();
        collection.Register<IEntityManager, EntityManager>();
        collection.Register<IAssetManager, AssetManager>();
        collection.Register<ISpriteManager, SpriteManager>();
        collection.Register<IAudioManager, AudioManager>();
        collection.Register<GameStrings, GameStrings>();
        collection.Register(collection);
    }
}