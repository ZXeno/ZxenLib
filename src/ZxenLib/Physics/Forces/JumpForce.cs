namespace ZxenLib.Physics.Forces;

using Components;
using Microsoft.Xna.Framework;

public class JumpForce : IForceGenerator
{
    private Vector2 jumpForce;

    public JumpForce()
    {
        this.jumpForce = new(0, 20);
    }

    public JumpForce(Vector2 jump)
    {
        this.jumpForce = jump;
    }

    /// <summary>
    /// Sets the force of a jump.
    /// </summary>
    /// <param name="force">The value to set the force of a jump.</param>
    public void SetForce(Vector2 force)
    {
        this.jumpForce = force;
    }

    public void UpdateForce(Rigidbody2D body, float deltaTime)
    {
        body.AddForce(this.jumpForce * body.Mass);
    }
}