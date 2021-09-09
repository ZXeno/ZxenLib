namespace ZxenLib.Input
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// Used to handle all input for the game.
    /// </summary>
    public class InputProvider
    {
        // This is for binding specific event handlers related to input.
        private static GameWindow gameWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputProvider"/> class.
        /// </summary>
        public InputProvider()
        {
        }

        /// <summary>
        /// Event that is fired when a TextInputEvent is received.
        /// </summary>
        public static event EventHandler<TextInputEventArgs> TextInputEvent;

        /// <summary>
        /// Gets or sets a value indicating whether the game window is currently active.
        /// </summary>
        public static bool WindowIsActive { get; set; }

        public static KeyboardController Keyboard { get; private set; }

        public static MouseController Mouse { get; private set; }

        /// <summary>
        /// Initializes the <see cref="InputProvider"/> class.
        /// </summary>
        /// <param name="window">The <see cref="GameWindow"/> to bind certain input events to.</param>
        public virtual void Initialize(GameWindow window)
        {
            if (window != null)
            {
                gameWindow = window;
                gameWindow.TextInput += TextInputPassthrough;
            }
        }

        /// <summary>
        /// Updates the input states of the <see cref="InputProvider"/> every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public virtual void Update(float deltaTime)
        {
            Keyboard.Update(deltaTime);
            Mouse.Update(deltaTime);
        }

        private static void TextInputPassthrough(object sender, TextInputEventArgs e)
        {
            TextInputEvent?.Invoke(sender, e);
        }
    }
}
