namespace ZxenLib.Physics.Forces;

public class ForceRegistration
{
    private readonly IForceGenerator? forceGenerator = null;
    private readonly Rigidbody2D? rigidbody = null;

    public ForceRegistration(IForceGenerator generator, Rigidbody2D rigidbody)
    {
        this.forceGenerator = generator;
        this.rigidbody = rigidbody;
    }

    public IForceGenerator? ForceGenerator => this.forceGenerator;

    public Rigidbody2D? Rigidbody => this.rigidbody;

    public override bool Equals(object? obj)
    {
        if (obj is not ForceRegistration otherForceReg)
        {
            return false;
        }

        return this.rigidbody == otherForceReg.rigidbody && this.forceGenerator == otherForceReg.forceGenerator;
    }
}