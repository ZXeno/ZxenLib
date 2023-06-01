namespace ZxenLib.Physics.Forces;

public interface IForceGenerator
{
    void UpdateForce(Rigidbody2D body, float deltaTime);
}