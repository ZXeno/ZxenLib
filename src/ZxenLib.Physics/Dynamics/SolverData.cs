namespace ZxenLib.Physics.Dynamics;

/// Solver Data
public ref struct SolverData
{
    public readonly TimeStep Step;

    public readonly Position[] Positions;

    public readonly Velocity[] Velocities;

    public SolverData(in TimeStep step, Position[] positions, Velocity[] velocities)
    {
        this.Step = step;
        this.Positions = positions;
        this.Velocities = velocities;
    }
}