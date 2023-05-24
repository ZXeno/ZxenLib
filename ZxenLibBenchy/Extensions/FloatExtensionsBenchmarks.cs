namespace ZxenLibBenchy.Extensions;

using BenchmarkDotNet.Attributes;
using ZxenLib.Extensions;

[MemoryDiagnoser]
public class FloatExtensionsBenchmarks
{
    private Random rnd;
    // public float[] checkvals;

    private float r1;
    private float r2;
    private float r3;
    private float r4;
    private float eps;

    public FloatExtensionsBenchmarks()
    {
        // int dataLength = 10000;
        // this.checkvals = new float[dataLength];
        this.rnd = new Random();

        // for (int x = 0; x < dataLength; x++)
        // {
        //     this.checkvals[x] = rnd.RangeSingle(-30000f, 30000f);
        // }
        this.r1 = this.rnd.NextSingle();
        this.r2 = this.r1 + 1;
        this.eps = this.rnd.NextSingle() * .5f;
        this.r3 = this.r1 + this.eps - this.eps * .02f;
        this.r4 = this.eps + this.r1;
    }

    [Benchmark]
    public bool CheckWithinThreshold_Single_False()
    {
        return this.r2.WithinThreshold(this.eps);
    }

    [Benchmark]
    public bool CheckWithinThreshold_Single_True()
    {
        return this.r3.WithinThreshold(this.r4);
    }

    [Benchmark]
    public bool CheckEpsilon_False()
    {
        return this.r1.Compare(this.r2, this.eps);
    }

    [Benchmark]
    public bool CheckEpsilon_True()
    {
        return this.r1.Compare(this.r3, this.eps);
    }
}