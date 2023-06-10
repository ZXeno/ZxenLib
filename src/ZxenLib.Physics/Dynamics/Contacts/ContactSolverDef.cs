namespace ZxenLib.Physics.Dynamics.Contacts;

public ref struct ContactSolverDef
{
    public readonly TimeStep Step;

    public readonly int ContactCount;

    public readonly Contact[] Contacts;

    public readonly Position[] Positions;

    public readonly Velocity[] Velocities;

    public ContactSolverDef(in TimeStep step, int contactCount, Contact[] contacts, Position[] positions, Velocity[] velocities)
    {
        this.Step = step;
        this.Contacts = contacts;
        this.ContactCount = contactCount;
        this.Positions = positions;
        this.Velocities = velocities;
    }
}