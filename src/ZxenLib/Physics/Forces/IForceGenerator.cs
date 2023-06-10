namespace ZxenLib.Physics.Forces;

using Components;

public interface IForceGenerator
{
    void UpdateForce(Rigidbody2D body, float deltaTime);
}