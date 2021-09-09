namespace ZxenLib.Input
{
    using Microsoft.Xna.Framework.Input;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class KeyboardController
    {
        public KeyboardController()
        {
            this.CurrentKeyboardState = Keyboard.GetState();
            this.LastKeyboardState = this.CurrentKeyboardState;
        }

        /// <summary>
        /// Gets the current <see cref="CurrentKeyboardState"/>.
        /// </summary>
        public KeyboardState CurrentKeyboardState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="CurrentKeyboardState"/>.
        /// </summary>
        public KeyboardState LastKeyboardState { get; private set; }

        public void Update(float deltaTime)
        {
            this.LastKeyboardState = this.CurrentKeyboardState;
            this.CurrentKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value was just lifted.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state changes from down to up. False if no change.</returns>
        public bool GetKeyUp(Keys key)
        {
            return this.CurrentKeyboardState.IsKeyUp(key) && this.LastKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value was just pressed.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state changes from up to down. False if no change.</returns>
        public bool GetKeyDown(Keys key)
        {
            return this.CurrentKeyboardState.IsKeyDown(key) && this.LastKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Determines if the provided <see cref="Keys"/> value is pressed down.
        /// </summary>
        /// <param name="key">The <see cref="Keys"/> value representing the key to check.</param>
        /// <returns>True if key state is down. False if not.</returns>
        public bool GetKey(Keys key)
        {
            return this.CurrentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Converts a key to its char representation.
        /// </summary>
        /// <param name="key">The key pressed.</param>
        /// <returns>A nullable <see cref="char"/> based on the key pressed.</returns>
        public char? ToChar(Keys key)
        {
            bool isShiftDown = this.GetKey(Keys.LeftShift) || this.GetKey(Keys.RightShift);

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
    }
}
