namespace ZxenLib.Physics.Forces;

using Microsoft.Xna.Framework;

public class Gravity2D : IForceGenerator
{
    private Vector2 gravityForce;

    public Gravity2D()
    {
        this.gravityForce = new();
    }

    public Gravity2D(Vector2 gravity)
    {
        this.gravityForce = gravity;
    }

    /// <summary>
    /// Sets the force of gravity.
    /// </summary>
    /// <param name="force">The value to set the force of gravity.</param>
    public void SetForce(Vector2 force)
    {
        this.gravityForce = force;
    }

    public void UpdateForce(Rigidbody2D body, float deltaTime)
    {
        body.AddForce(this.gravityForce * body.Mass);
    }
}