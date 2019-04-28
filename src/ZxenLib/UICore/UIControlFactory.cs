namespace ZxenLib.UICore
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using ZxenLib.Graphics;
    using ZxenLib.Managers;

    /// <summary>
    /// Factory class to build UI controls with default values.
    /// </summary>
    public class UIControlFactory
    {
        private readonly IAssetManager assetManager;
        private readonly ISpriteManager spriteManager;
        private string uiAtlasName = string.Empty;
        private string uiFontName = string.Empty;
        private Texture2D uiSpriteSheet;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIControlFactory"/> class.
        /// </summary>
        /// <param name="uiAtlasName">The name of the UI sprite atlas.</param>
        /// <param name="uiFontName">The name of the UI font.</param>
        /// <param name="assetManager">The game's <see cref="IAssetManager"/> instance.</param>
        /// <param name="spriteManager">The game's <see cref="ISpriteManager"/> instance.</param>
        public UIControlFactory(string uiAtlasName, string uiFontName, IAssetManager assetManager, ISpriteManager spriteManager)
        {
            this.assetManager = assetManager;
            this.spriteManager = spriteManager;
            this.uiAtlasName = uiAtlasName;
            this.uiFontName = uiFontName;

            UIControlFactory.Instance = this;
        }

        /// <summary>
        /// Gets the instance of this class.
        /// </summary>
        public static UIControlFactory Instance { get; private set; }

        /// <summary>
        /// Gets a value indicating whether initialization has processed.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets the default UI font.
        /// </summary>
        public SpriteFont UiDefaultSpriteFont { get; set; }

        /// <summary>
        /// Gets or sets the default button sprite.
        /// </summary>
        public Sprite DefaultButtonSprite { get; set; }

        /// <summary>
        /// Gets or sets the default window sprite.
        /// </summary>
        public Sprite DefaultWindowSprite { get; set; }

        /// <summary>
        /// Gets or sets the default window close button sprite.
        /// </summary>
        public Sprite DefaultWindowCloseButtonSprite { get; set; }

        /// <summary>
        /// Initialzies the properties of the <see cref="UIControlFactory"/>
        /// </summary>
        public void Initialize()
        {
            // TODO: for modding purposes, perhaps create a UI data file in JSON that defines these names? Could be easier. Maybe.
            this.UiDefaultSpriteFont = this.assetManager.Fonts[this.uiFontName];
            this.DefaultButtonSprite = this.spriteManager.GetSprite(this.uiAtlasName, "DefaultButtonBackground");
            this.DefaultWindowSprite = this.spriteManager.GetSprite(this.uiAtlasName, "Window");
            this.DefaultWindowCloseButtonSprite = this.spriteManager.GetSprite(this.uiAtlasName, "CloseButton");
            this.uiSpriteSheet = this.spriteManager.GetAtlas(this.uiAtlasName).TextureAtlas;

            this.IsInitialized = true;
        }

        /// <summary>
        /// Creates a <see cref="Window"/> with defaults.
        /// </summary>
        /// <param name="uiPane">Required parent <see cref="IUIPane"/>.</param>
        /// <param name="childControls">Optional list of child controls for the window.</param>
        /// <param name="windowTitle">Optional name of the <see cref="Window"/> title.</param>
        /// <returns>A default <see cref="Window"/>. Will return null if uiPane is null.</returns>
        public Window CreateWindow(IUIPane uiPane, IList<IControl> childControls = null, string windowTitle = "")
        {
            if (uiPane == null)
            {
                return null;
            }

            if (childControls == null)
            {
                childControls = new List<IControl>();
            }

            Window windowObject = new Window(uiPane, this.DefaultWindowSprite, this.DefaultWindowCloseButtonSprite, this.uiSpriteSheet, this.UiDefaultSpriteFont, default(Point), windowTitle, 150, 150, uiPane.BoundingRectangle);
            if (childControls.Count > 0)
            {
                foreach (IControl control in childControls)
                {
                    windowObject.AddChildControl(control);
                }
            }

            return windowObject;
        }

        /// <summary>
        /// Creates a <see cref="Button"/> with defaults.
        /// </summary>
        /// <param name="uiPane">Required parent <see cref="IUIPane"/>.</param>
        /// <param name="buttonText">Optional text of the button.</param>
        /// <returns>A default <see cref="Button"/>. Will return null if uiPane is null.</returns>
        public Button CreateButton(IUIPane uiPane, string buttonText = null)
        {
            if (uiPane == null)
            {
                return null;
            }

            Button buttonObject = new Button(uiPane, this.DefaultButtonSprite, this.uiSpriteSheet, default(Point), this.UiDefaultSpriteFont, uiPane.BoundingRectangle);
            buttonObject.SetText(buttonText);
            buttonObject.StretchBGImage = true;

            return buttonObject;
        }

        /// <summary>
        /// Creates a <see cref="Label"/> with defaults.
        /// </summary>
        /// <param name="uiPane">Required parent <see cref="IUIPane"/>.</param>
        /// <param name="labelText">The label text.</param>
        /// <returns>A default <see cref="Label"/>. Will return null if uiPane is null.</returns>
        public Label CreateLabel(IUIPane uiPane, string labelText)
        {
            if (uiPane == null)
            {
                return null;
            }

            Label labelObject = new Label(uiPane, this.UiDefaultSpriteFont, labelText, default(Point), uiPane.BoundingRectangle);

            return labelObject;
        }

        /// <summary>
        /// Creates a <see cref="ListGroup"/> with defaults.
        /// </summary>
        /// <param name="uiPane">Required parent <see cref="IUIPane"/>.</param>
        /// <param name="childControls">Optional list of child controls for the <see cref="ListGroup"/>.</param>
        /// <returns>A default <see cref="ListGroup"/>. Will return null if uiPane is null.</returns>
        public ListGroup CreateListGroup(IUIPane uiPane, IList<IControl> childControls = null)
        {
            if (uiPane == null)
            {
                return null;
            }

            if (childControls == null)
            {
                childControls = new List<IControl>();
            }

            ListGroup listGroupObject = new ListGroup(uiPane, default(Point), uiPane.BoundingRectangle.Width, uiPane.BoundingRectangle.Height);
            if (childControls.Count > 0)
            {
                foreach (IControl control in childControls)
                {
                    listGroupObject.AddControl(control);
                }
            }

            return listGroupObject;
        }
    }
}
