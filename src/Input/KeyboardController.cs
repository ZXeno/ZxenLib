namespace ZxenLib.Input;

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

        return isShiftDown ? this.ShiftedKeysToCharMap[key] : this.KeyToCharMap[key];
    }

    private Dictionary<Keys, char> ShiftedKeysToCharMap = new()
    {
        { Keys.A, 'A'},
        { Keys.B, 'B'},
        { Keys.C, 'C'},
        { Keys.D, 'D'},
        { Keys.E, 'E'},
        { Keys.F, 'F'},
        { Keys.G, 'G'},
        { Keys.H, 'H'},
        { Keys.I, 'I'},
        { Keys.J, 'J'},
        { Keys.K, 'K'},
        { Keys.L, 'L'},
        { Keys.M, 'M'},
        { Keys.N, 'N'},
        { Keys.O, 'O'},
        { Keys.P, 'P'},
        { Keys.Q, 'Q'},
        { Keys.R, 'R'},
        { Keys.S, 'S'},
        { Keys.T, 'T'},
        { Keys.U, 'U'},
        { Keys.V, 'V'},
        { Keys.W, 'W'},
        { Keys.X, 'X'},
        { Keys.Y, 'Y'},
        { Keys.Z, 'Z'},
        { Keys.D0, ')'},
        { Keys.D1, '!'},
        { Keys.D2, '@'},
        { Keys.D3, '#'},
        { Keys.D4, '$'},
        { Keys.D5, '%'},
        { Keys.D6, '^'},
        { Keys.D7, '&'},
        { Keys.D8, '*'},
        { Keys.D9, '('},
        { Keys.OemSemicolon, ':' },
        { Keys.OemPlus, '+' },
        { Keys.OemComma, '<' },
        { Keys.OemMinus, '_' },
        { Keys.OemPeriod, '>' },
        { Keys.OemQuestion, '?' },
        { Keys.OemTilde, '~' },
        { Keys.OemOpenBrackets, '{' },
        { Keys.OemCloseBrackets, '}' },
        { Keys.OemPipe, '|' },
        { Keys.OemQuotes, '\"'},
    };

    private Dictionary<Keys, char> KeyToCharMap = new()
    {
        { Keys.Tab, '\t'},
        { Keys.Enter, (char)13},
        { Keys.Space, ' '},
        { Keys.Back, (char)8},
        { Keys.A, 'a'},
        { Keys.B, 'b'},
        { Keys.C, 'c'},
        { Keys.D, 'd'},
        { Keys.E, 'e'},
        { Keys.F, 'f'},
        { Keys.G, 'g'},
        { Keys.H, 'h'},
        { Keys.I, 'i'},
        { Keys.J, 'j'},
        { Keys.K, 'k'},
        { Keys.L, 'l'},
        { Keys.M, 'm'},
        { Keys.N, 'n'},
        { Keys.O, 'o'},
        { Keys.P, 'p'},
        { Keys.Q, 'q'},
        { Keys.R, 'r'},
        { Keys.S, 's'},
        { Keys.T, 't'},
        { Keys.U, 'u'},
        { Keys.V, 'v'},
        { Keys.W, 'w'},
        { Keys.X, 'x'},
        { Keys.Y, 'y'},
        { Keys.Z, 'z'},
        { Keys.NumPad0, '0'},
        { Keys.NumPad1, '1'},
        { Keys.NumPad2, '2'},
        { Keys.NumPad3, '3'},
        { Keys.NumPad4, '4'},
        { Keys.NumPad5, '5'},
        { Keys.NumPad6, '6'},
        { Keys.NumPad7, '7'},
        { Keys.NumPad8, '8'},
        { Keys.NumPad9, '9'},
        { Keys.D0, '0'},
        { Keys.D1, '1'},
        { Keys.D2, '2'},
        { Keys.D3, '3'},
        { Keys.D4, '4'},
        { Keys.D5, '5'},
        { Keys.D6, '6'},
        { Keys.D7, '7'},
        { Keys.D8, '8'},
        { Keys.D9, '9'},
        { Keys.Multiply, '*'},
        { Keys.Add, '+'},
        { Keys.Subtract, '-'},
        { Keys.Decimal, '.'},
        { Keys.Divide, '/'},
        { Keys.OemSemicolon, ';'},
        { Keys.OemPlus, '='},
        { Keys.OemComma, ','},
        { Keys.OemMinus, '-'},
        { Keys.OemPeriod, '.'},
        { Keys.OemQuestion, '/'},
        { Keys.OemTilde, '`'},
        { Keys.OemOpenBrackets, '['},
        { Keys.OemPipe, '\\'},
        { Keys.OemCloseBrackets, ']'},
        { Keys.OemQuotes,  '\''},
        { Keys.OemBackslash, '\\'},
    };
}