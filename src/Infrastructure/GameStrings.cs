namespace ZxenLib.Infrastructure
{
    /// <summary>
    /// Wrapper class for getting loaded strings values.
    /// </summary>
    public class GameStrings
    {
        private const string MissingText = "TEXT_NOT_FOUND";

        /// <summary>
        /// Initializes a new instance of the <see cref="GameStrings"/> class.
        /// </summary>
        /// <param name="assetManager">The game's <see cref="IAssetManager"/> dependency instance.</param>
        public GameStrings(IAssetManager assetManager)
        {
            AssetManager = assetManager;
        }

        /// <summary>
        /// Gets the AssetManager dependency.
        /// </summary>
        protected static IAssetManager AssetManager { get; private set; }

        /// <summary>
        /// Returns the localized string.
        /// </summary>
        /// <param name="stringId">The key ID of the localized string requested.</param>
        /// <returns>The localized string from the asset manager's strings dictionary. Returns "TEXT_NOT_FOUND" if. </returns>
        public static string GetLocalizedString(string stringId)
        {
            if (AssetManager != null)
            {
                return AssetManager.Strings?[stringId] ?? MissingText;
            }

            return MissingText;
        }
    }
}
