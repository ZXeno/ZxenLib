namespace ZxenLib.Input
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;

    public class MouseController
    {
        public MouseController()
        {
            this.CurrentMouseState = Mouse.GetState();
            this.LastMouseState = this.CurrentMouseState;
        }

        /// <summary>
        /// Gets the current <see cref="MouseState"/>.
        /// </summary>
        public MouseState CurrentMouseState { get; private set; }

        /// <summary>
        /// Gets the previous <see cref="MouseState"/>.
        /// </summary>
        public MouseState LastMouseState { get; private set; }

        /// <summary>
        ///  Gets the current mouse position as a <see cref="Point"/> coordinate.
        /// </summary>
        public Point MousePosition => this.CurrentMouseState.Position;

        public void Update(float deltaTime)
        {
            this.LastMouseState = this.CurrentMouseState;
            this.CurrentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Determines if the provided mouse button was just pressed down.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just pressed down. False if not.</returns>
        public bool GetButtonDown(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => (this.CurrentMouseState.LeftButton == ButtonState.Pressed) && (this.LastMouseState.LeftButton == ButtonState.Released),
                1 => (this.CurrentMouseState.RightButton == ButtonState.Pressed) && (this.LastMouseState.RightButton == ButtonState.Released),
                2 => (this.CurrentMouseState.MiddleButton == ButtonState.Pressed) && (this.LastMouseState.MiddleButton == ButtonState.Released),
                3 => (this.CurrentMouseState.XButton1 == ButtonState.Pressed) && (this.LastMouseState.XButton1 == ButtonState.Released),
                4 => (this.CurrentMouseState.XButton2 == ButtonState.Pressed) && (this.LastMouseState.XButton2 == ButtonState.Released),
                _ => false,
            };
        }

        /// <summary>
        /// Determines if the provided mouse button was just released.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button was just released. False if not.</returns>
        public bool GetButtonUp(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => (this.CurrentMouseState.LeftButton == ButtonState.Released) && (this.LastMouseState.LeftButton == ButtonState.Pressed),
                1 => (this.CurrentMouseState.RightButton == ButtonState.Released) && (this.LastMouseState.RightButton == ButtonState.Pressed),
                2 => (this.CurrentMouseState.MiddleButton == ButtonState.Released) && (this.LastMouseState.MiddleButton == ButtonState.Pressed),
                3 => (this.CurrentMouseState.XButton1 == ButtonState.Released) && (this.LastMouseState.XButton1 == ButtonState.Pressed),
                4 => (this.CurrentMouseState.XButton2 == ButtonState.Released) && (this.LastMouseState.XButton2 == ButtonState.Pressed),
                _ => false,
            };
        }

        /// <summary>
        /// Determines if the provided mouse button is currently down.
        /// 0 = Left mouse button, 1 = Right mouse button, 2 = middle mouse button, 3 = XButton1, 4 = XButton 5.
        /// </summary>
        /// <param name="button">Int value represenint the corresponding mouse button.</param>
        /// <returns>True if the provided button is down. False if not.</returns>
        public bool GetButton(int button)
        {
            if (button > 4)
            {
                return false;
            }

            return button switch
            {
                0 => this.CurrentMouseState.LeftButton == ButtonState.Pressed,
                1 => this.CurrentMouseState.RightButton == ButtonState.Pressed,
                2 => this.CurrentMouseState.MiddleButton == ButtonState.Pressed,
                3 => this.CurrentMouseState.XButton1 == ButtonState.Pressed,
                4 => this.CurrentMouseState.XButton2 == ButtonState.Pressed,
                _ => false,
            };
        }
    }
}
