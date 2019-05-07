namespace ZxenLib.Input
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// Used to handle all input for the game.
    /// </summary>
    public class InputWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputWrapper"/> class.
        /// </summary>
        public InputWrapper()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the game window is currently active.
        /// </summary>
        public static bool WindowIsActive { get; set; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.Xna.Framework.Input.KeyboardState"/>
        /// </summary>
        public static KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="Microsoft.Xna.Framework.Input.KeyboardState"/>
        /// </summary>
        public static KeyboardState LastKeyboardState { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Microsoft.Xna.Framework.Input.MouseState"/>
        /// </summary>
        public static MouseState MouseState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="Microsoft.Xna.Framework.Input.MouseState"/>
        /// </summary>
        public static MouseState LastMouseState { get; private set; }

        /// <summary>
        ///  Gets the current mouse position as a <see cref="Point"/> coordinate.
        /// </summary>
        public static Point MousePosition => InputWrapper.MouseState.Position;

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
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just pressed down. False if not.</returns>
        public static bool GetButtonDown(int button)
        {
            if (button > 4)
            {
                return false;
            }

            switch (button)
            {
                case 0:
                    return (MouseState.LeftButton == ButtonState.Pressed) && (LastMouseState.LeftButton == ButtonState.Released);
                case 1:
                    return (MouseState.RightButton == ButtonState.Pressed) && (LastMouseState.RightButton == ButtonState.Released);
                case 2:
                    return (MouseState.MiddleButton == ButtonState.Pressed) && (LastMouseState.MiddleButton == ButtonState.Released);
                case 3:
                    return (MouseState.XButton1 == ButtonState.Pressed) && (LastMouseState.XButton1 == ButtonState.Released);
                case 4:
                    return (MouseState.XButton2 == ButtonState.Pressed) && (LastMouseState.XButton2 == ButtonState.Released);
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided mouse button was just released.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just released. False if not.</returns>
        public static bool GetButtonUp(int button)
        {
            if (button > 4)
            {
                return false;
            }

            switch (button)
            {
                case 0:
                    return (MouseState.LeftButton == ButtonState.Released) && (LastMouseState.LeftButton == ButtonState.Pressed);
                case 1:
                    return (MouseState.RightButton == ButtonState.Released) && (LastMouseState.RightButton == ButtonState.Pressed);
                case 2:
                    return (MouseState.MiddleButton == ButtonState.Released) && (LastMouseState.MiddleButton == ButtonState.Pressed);
                case 3:
                    return (MouseState.XButton1 == ButtonState.Released) && (LastMouseState.XButton1 == ButtonState.Pressed);
                case 4:
                    return (MouseState.XButton2 == ButtonState.Released) && (LastMouseState.XButton2 == ButtonState.Pressed);
            }

            return false;
        }

        /// <summary>
        /// Determines if the provided mouse button was currently down.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button is down. False if not.</returns>
        public static bool GetButton(int button)
        {
            if (button > 4)
            {
                return false;
            }

            switch (button)
            {
                case 0:
                    return MouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return MouseState.RightButton == ButtonState.Pressed;
                case 2:
                    return MouseState.MiddleButton == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1 == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2 == ButtonState.Pressed;
            }

            return false;
        }

        /// <summary>
        /// Converts a key to its char representation
        /// </summary>
        /// <param name="key">The key pressed.</param>
        /// <param name="isShiftDown"><see cref="bool"/> indicating whether the shift key is currently being held.</param>
        /// <returns>A nullable <see cref="char"/> based on the key pressed.</returns>
        public static char? ToChar(Keys key, bool isShiftDown)
        {
            if (key == Keys.A) { return isShiftDown ? 'A' : 'a'; }
            if (key == Keys.B) { return isShiftDown ? 'B' : 'b'; }
            if (key == Keys.C) { return isShiftDown ? 'C' : 'c'; }
            if (key == Keys.D) { return isShiftDown ? 'D' : 'd'; }
            if (key == Keys.E) { return isShiftDown ? 'E' : 'e'; }
            if (key == Keys.F) { return isShiftDown ? 'F' : 'f'; }
            if (key == Keys.G) { return isShiftDown ? 'G' : 'g'; }
            if (key == Keys.H) { return isShiftDown ? 'H' : 'h'; }
            if (key == Keys.I) { return isShiftDown ? 'I' : 'i'; }
            if (key == Keys.J) { return isShiftDown ? 'J' : 'j'; }
            if (key == Keys.K) { return isShiftDown ? 'K' : 'k'; }
            if (key == Keys.L) { return isShiftDown ? 'L' : 'l'; }
            if (key == Keys.M) { return isShiftDown ? 'M' : 'm'; }
            if (key == Keys.N) { return isShiftDown ? 'N' : 'n'; }
            if (key == Keys.O) { return isShiftDown ? 'O' : 'o'; }
            if (key == Keys.P) { return isShiftDown ? 'P' : 'p'; }
            if (key == Keys.Q) { return isShiftDown ? 'Q' : 'q'; }
            if (key == Keys.R) { return isShiftDown ? 'R' : 'r'; }
            if (key == Keys.S) { return isShiftDown ? 'S' : 's'; }
            if (key == Keys.T) { return isShiftDown ? 'T' : 't'; }
            if (key == Keys.U) { return isShiftDown ? 'U' : 'u'; }
            if (key == Keys.V) { return isShiftDown ? 'V' : 'v'; }
            if (key == Keys.W) { return isShiftDown ? 'W' : 'w'; }
            if (key == Keys.X) { return isShiftDown ? 'X' : 'x'; }
            if (key == Keys.Y) { return isShiftDown ? 'Y' : 'y'; }
            if (key == Keys.Z) { return isShiftDown ? 'Z' : 'z'; }

            if ((key == Keys.D0 && !isShiftDown) || key == Keys.NumPad0) { return '0'; }
            if ((key == Keys.D1 && !isShiftDown) || key == Keys.NumPad1) { return '1'; }
            if ((key == Keys.D2 && !isShiftDown) || key == Keys.NumPad2) { return '2'; }
            if ((key == Keys.D3 && !isShiftDown) || key == Keys.NumPad3) { return '3'; }
            if ((key == Keys.D4 && !isShiftDown) || key == Keys.NumPad4) { return '4'; }
            if ((key == Keys.D5 && !isShiftDown) || key == Keys.NumPad5) { return '5'; }
            if ((key == Keys.D6 && !isShiftDown) || key == Keys.NumPad6) { return '6'; }
            if ((key == Keys.D7 && !isShiftDown) || key == Keys.NumPad7) { return '7'; }
            if ((key == Keys.D8 && !isShiftDown) || key == Keys.NumPad8) { return '8'; }
            if ((key == Keys.D9 && !isShiftDown) || key == Keys.NumPad9) { return '9'; }

            if (key == Keys.D0 && isShiftDown) { return ')'; }
            if (key == Keys.D1 && isShiftDown) { return '!'; }
            if (key == Keys.D2 && isShiftDown) { return '@'; }
            if (key == Keys.D3 && isShiftDown) { return '#'; }
            if (key == Keys.D4 && isShiftDown) { return '$'; }
            if (key == Keys.D5 && isShiftDown) { return '%'; }
            if (key == Keys.D6 && isShiftDown) { return '^'; }
            if (key == Keys.D7 && isShiftDown) { return '&'; }
            if (key == Keys.D8 && isShiftDown) { return '*'; }
            if (key == Keys.D9 && isShiftDown) { return '('; }

            if (key == Keys.Space) { return ' '; }
            if (key == Keys.Tab) { return '\t'; }
            if (key == Keys.Enter) { return (char)13; }
            if (key == Keys.Back) { return (char)8; }

            if (key == Keys.Add) { return '+'; }
            if (key == Keys.Decimal) { return '.'; }
            if (key == Keys.Divide) { return '/'; }
            if (key == Keys.Multiply) { return '*'; }
            if (key == Keys.OemBackslash) { return '\\'; }
            if (key == Keys.OemComma && !isShiftDown) { return ','; }
            if (key == Keys.OemComma && isShiftDown) { return '<'; }
            if (key == Keys.OemOpenBrackets && !isShiftDown) { return '['; }
            if (key == Keys.OemOpenBrackets && isShiftDown) { return '{'; }
            if (key == Keys.OemCloseBrackets && !isShiftDown) { return ']'; }
            if (key == Keys.OemCloseBrackets && isShiftDown) { return '}'; }
            if (key == Keys.OemPeriod && !isShiftDown) { return '.'; }
            if (key == Keys.OemPeriod && isShiftDown) { return '>'; }
            if (key == Keys.OemPipe && !isShiftDown) { return '\\'; }
            if (key == Keys.OemPipe && isShiftDown) { return '|'; }
            if (key == Keys.OemPlus && !isShiftDown) { return '='; }
            if (key == Keys.OemPlus && isShiftDown) { return '+'; }
            if (key == Keys.OemMinus && !isShiftDown) { return '-'; }
            if (key == Keys.OemMinus && isShiftDown) { return '_'; }
            if (key == Keys.OemQuestion && !isShiftDown) { return '/'; }
            if (key == Keys.OemQuestion && isShiftDown) { return '?'; }
            if (key == Keys.OemQuotes && !isShiftDown) { return '\''; }
            if (key == Keys.OemQuotes && isShiftDown) { return '"'; }
            if (key == Keys.OemSemicolon && !isShiftDown) { return ';'; }
            if (key == Keys.OemSemicolon && isShiftDown) { return ':'; }
            if (key == Keys.OemTilde && !isShiftDown) { return '`'; }
            if (key == Keys.OemTilde && isShiftDown) { return '~'; }
            if (key == Keys.Subtract) { return '-'; }

            return null;
        }

        /// <summary>
        /// Initializes the <see cref="InputWrapper"/> class.
        /// </summary>
        public virtual void Initialize()
        {
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            LastKeyboardState = KeyboardState;
            LastMouseState = MouseState;
        }

        /// <summary>
        /// Updates the input states of the <see cref="InputWrapper"/> every frame.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
        public virtual void Update(float deltaTime)
        {
            LastKeyboardState = KeyboardState;
            LastMouseState = MouseState;
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
        }
    }
}
