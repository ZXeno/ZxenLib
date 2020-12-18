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

        /// <summary>
        /// Gets the current <see cref="Microsoft.Xna.Framework.Input.KeyboardState"/>.
        /// </summary>
        public static KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="Microsoft.Xna.Framework.Input.KeyboardState"/>.
        /// </summary>
        public static KeyboardState LastKeyboardState { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.Xna.Framework.Input.MouseState"/>.
        /// </summary>
        public static MouseState MouseState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="Microsoft.Xna.Framework.Input.MouseState"/>.
        /// </summary>
        public static MouseState LastMouseState { get; private set; }

        /// <summary>
        ///  Gets the current mouse position as a <see cref="Point"/> coordinate.
        /// </summary>
        public static Point MousePosition => InputProvider.MouseState.Position;

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value was just lifted.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state changes from down to up. False if no change.</returns>
        public static bool GetKeyUp(Keys key)
        {
            return KeyboardState.IsKeyUp(key) && LastKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value was just pressed.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state changes from up to down. False if no change.</returns>
        public static bool GetKeyDown(Keys key)
        {
            return KeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value is pressed down.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state is down. False if not.</returns>
        public static bool GetKey(Keys key)
        {
            return KeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines if the provided mouse button was just pressed down.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just pressed down. False if not.</returns>
        public static bool GetButtonDown(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => (MouseState.LeftButton == ButtonState.Pressed) && (LastMouseState.LeftButton == ButtonState.Released),
                1 => (MouseState.RightButton == ButtonState.Pressed) && (LastMouseState.RightButton == ButtonState.Released),
                2 => (MouseState.MiddleButton == ButtonState.Pressed) && (LastMouseState.MiddleButton == ButtonState.Released),
                3 => (MouseState.XButton1 == ButtonState.Pressed) && (LastMouseState.XButton1 == ButtonState.Released),
                4 => (MouseState.XButton2 == ButtonState.Pressed) && (LastMouseState.XButton2 == ButtonState.Released),
                _ => false,
            };
        }

        /// <summary>
        /// Determines if the provided mouse button was just released.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just released. False if not.</returns>
        public static bool GetButtonUp(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => (MouseState.LeftButton == ButtonState.Released) && (LastMouseState.LeftButton == ButtonState.Pressed),
                1 => (MouseState.RightButton == ButtonState.Released) && (LastMouseState.RightButton == ButtonState.Pressed),
                2 => (MouseState.MiddleButton == ButtonState.Released) && (LastMouseState.MiddleButton == ButtonState.Pressed),
                3 => (MouseState.XButton1 == ButtonState.Released) && (LastMouseState.XButton1 == ButtonState.Pressed),
                4 => (MouseState.XButton2 == ButtonState.Released) && (LastMouseState.XButton2 == ButtonState.Pressed),
                _ => false,
            };
        }

        /// <summary>
        /// Determines if the provided mouse button is currently down.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button is down. False if not.</returns>
        public static bool GetButton(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => MouseState.LeftButton == ButtonState.Pressed,
                1 => MouseState.RightButton == ButtonState.Pressed,
                2 => MouseState.MiddleButton == ButtonState.Pressed,
                3 => MouseState.XButton1 == ButtonState.Pressed,
                4 => MouseState.XButton2 == ButtonState.Pressed,
                _ => false,
            };
        }

        /// <summary>
        /// Converts a key to its char representation.
        /// </summary>
        /// <param name="key">The key pressed.</param>
        /// <returns>A nullable <see cref="char"/> based on the key pressed.</returns>
        public static char? ToChar(Keys key)
        {
            bool isShiftDown = GetKey(Keys.LeftShift) || GetKey(Keys.RightShift);

            return key switch
            {
                Keys.Tab => '\t',
                Keys.Enter => (char)13,
                Keys.Space => ' ',
                Keys.Back => (char)8,
                Keys.A => isShiftDown ? 'A' : 'a',
                Keys.B => isShiftDown ? 'B' : 'b',
                Keys.C => isShiftDown ? 'C' : 'c',
                Keys.D => isShiftDown ? 'D' : 'd',
                Keys.E => isShiftDown ? 'E' : 'e',
                Keys.F => isShiftDown ? 'F' : 'f',
                Keys.G => isShiftDown ? 'G' : 'g',
                Keys.H => isShiftDown ? 'H' : 'h',
                Keys.I => isShiftDown ? 'I' : 'i',
                Keys.J => isShiftDown ? 'J' : 'j',
                Keys.K => isShiftDown ? 'K' : 'k',
                Keys.L => isShiftDown ? 'L' : 'l',
                Keys.M => isShiftDown ? 'M' : 'm',
                Keys.N => isShiftDown ? 'N' : 'n',
                Keys.O => isShiftDown ? 'O' : 'o',
                Keys.P => isShiftDown ? 'P' : 'p',
                Keys.Q => isShiftDown ? 'Q' : 'q',
                Keys.R => isShiftDown ? 'R' : 'r',
                Keys.S => isShiftDown ? 'S' : 's',
                Keys.T => isShiftDown ? 'T' : 't',
                Keys.U => isShiftDown ? 'U' : 'u',
                Keys.V => isShiftDown ? 'V' : 'v',
                Keys.W => isShiftDown ? 'W' : 'w',
                Keys.X => isShiftDown ? 'X' : 'x',
                Keys.Y => isShiftDown ? 'Y' : 'y',
                Keys.Z => isShiftDown ? 'Z' : 'z',
                Keys.NumPad0 => '0',
                Keys.NumPad1 => '1',
                Keys.NumPad2 => '2',
                Keys.NumPad3 => '3',
                Keys.NumPad4 => '4',
                Keys.NumPad5 => '5',
                Keys.NumPad6 => '6',
                Keys.NumPad7 => '7',
                Keys.NumPad8 => '8',
                Keys.NumPad9 => '9',
                Keys.D0 => isShiftDown ? ')' : '0',
                Keys.D1 => isShiftDown ? '!' : '1',
                Keys.D2 => isShiftDown ? '@' : '2',
                Keys.D3 => isShiftDown ? '#' : '3',
                Keys.D4 => isShiftDown ? '$' : '4',
                Keys.D5 => isShiftDown ? '%' : '5',
                Keys.D6 => isShiftDown ? '^' : '6',
                Keys.D7 => isShiftDown ? '&' : '7',
                Keys.D8 => isShiftDown ? '*' : '8',
                Keys.D9 => isShiftDown ? '(' : '9',
                Keys.Multiply => '*',
                Keys.Add => '+',
                Keys.Subtract => '-',
                Keys.Decimal => '.',
                Keys.Divide => '/',
                Keys.OemSemicolon => isShiftDown ? ':' : ';',
                Keys.OemPlus => isShiftDown ? '+' : '=',
                Keys.OemComma => isShiftDown ? '<' : ',',
                Keys.OemMinus => isShiftDown ? '_' : '-',
                Keys.OemPeriod => isShiftDown ? '>' : '.',
                Keys.OemQuestion => isShiftDown ? '?' : '/',
                Keys.OemTilde => isShiftDown ? '~' : '`',
                Keys.OemOpenBrackets => isShiftDown ? '{' : '[',
                Keys.OemPipe => isShiftDown ? '|' : '\\',
                Keys.OemCloseBrackets => isShiftDown ? '}' : ']',
                Keys.OemQuotes => isShiftDown ? '\"' : '\'',
                Keys.OemBackslash => '\\',
                _ => null,
            };
        }

        /// <summary>
        /// Initializes the <see cref="InputProvider"/> class.
        /// </summary>
        /// <param name="window">The <see cref="GameWindow"/> to bind certain input events to.</param>
        public virtual void Initialize(GameWindow window)
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            LastKeyboardState = KeyboardState;
            LastMouseState = MouseState;

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
            LastKeyboardState = KeyboardState;
            LastMouseState = MouseState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
        }

        private static void TextInputPassthrough(object sender, TextInputEventArgs e)
        {
            TextInputEvent?.Invoke(sender, e);
        }
    }
}
