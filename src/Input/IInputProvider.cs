namespace ZxenLib.Input;

using Microsoft.Xna.Framework;

public interface IInputProvider
{
    /// <summary>
    /// Initializes the <see cref="InputProvider"/> class.
    /// </summary>
    /// <param name="window">The <see cref="GameWindow"/> to bind certain input events to.</param>
    void Initialize(GameWindow window);

    /// <summary>
    /// Updates the input states of the <see cref="InputProvider"/> every frame.
    /// </summary>
    /// <param name="deltaTime">Elapsed frame time of the previous frame.</param>
    /// <param name="windowIsActive">Should be set from the Game class, determines if window is active or not.</param>
    void Update(float deltaTime, bool windowIsActive);
}