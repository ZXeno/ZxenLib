namespace ZxenLib
{
    using Microsoft.Extensions.DependencyInjection;
    using ZxenLib.Audio;
    using ZxenLib.Entities;
    using ZxenLib.Events;
    using ZxenLib.GameScreen;
    using ZxenLib.Graphics;
    using ZxenLib.Infrastructure;
    using ZxenLib.Input;

    /// <summary>
    /// Extension class to add all types to the Microsoft.Extensions.DependencyInjection container.
    /// </summary>
    public static class MicrosoftDependencyInjectionBuilder
    {
        /// <summary>
        /// Adds the various components of the ZxenLib library to the <see cref="IServiceCollection"/> of the Microsoft.Extensions.DependencyInjection library.
        /// </summary>
        /// <param name="collection">The <see cref="IServiceCollection"/> to register the various types to.</param>
        public static void AddZxenLib(this IServiceCollection collection)
        {
            collection.AddSingleton<ConfigurationManager>();
            collection.AddSingleton<InputProvider>();
            collection.AddSingleton<GameScreenManager>();
            collection.AddSingleton<IEventDispatcher, EventDispatcher>();
            collection.AddSingleton<IEntityManager, EntityManager>();
            collection.AddSingleton<IAssetManager, AssetManager>();
            collection.AddSingleton<ISpriteManager, SpriteManager>();
            collection.AddSingleton<ISFXManager, SFXManager>();
            collection.AddSingleton<GameStrings, GameStrings>();
        }
    }
}
