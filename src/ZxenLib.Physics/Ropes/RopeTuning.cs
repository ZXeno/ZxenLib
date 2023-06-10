namespace ZxenLib.Physics.Ropes;

///
public class RopeTuning
{
    public RopeTuning()
    {
        this.StretchingModel = StretchingModel.PbdStretchingModel;
        this.BendingModel = BendingModel.PbdAngleBendingModel;
        this.Damping = 0.0f;
        this.StretchStiffness = 1.0f;
        this.StretchHertz = 1.0f;
        this.StretchDamping = 0.0f;
        this.BendStiffness = 0.5f;
        this.BendHertz = 1.0f;
        this.BendDamping = 0.0f;
        this.Isometric = false;
        this.FixedEffectiveMass = false;
        this.WarmStart = false;
    }

    public StretchingModel StretchingModel;

    public BendingModel BendingModel;

    public float Damping;

    public float StretchStiffness;

    public float StretchHertz;

    public float StretchDamping;

    public float BendStiffness;

    public float BendHertz;

    public float BendDamping;

    public bool Isometric;

    public bool FixedEffectiveMass;

    public bool WarmStart;
};