namespace ZxenLib.Physics;

using System.Collections.Generic;
using Microsoft.Xna.Framework;

public class QuadTree
{
    public int MaxObjects { get; set; }

    public int MaxLevels { get; set; }


    private readonly int level;
    private readonly List<Rectangle> objects;
    private readonly QuadTree[] nodes;
    private Rectangle bounds;

    public QuadTree(int pLevel, Rectangle pBounds)
    {
        this.level = pLevel;
        this.objects = new List<Rectangle>();
        this.bounds = pBounds;
        this.nodes = new QuadTree[4];
        this.MaxObjects = 10;
        this.MaxLevels = 5;
    }

    public void Clear()
    {
        this.objects.Clear();

        for (int i = 0; i < this.nodes.Length; i++)
        {
            if (this.nodes[i] == null!)
            {
                continue;
            }

            this.nodes[i].Clear();
            this.nodes[i] = null!;
        }
    }

    private void Split()
    {
        int subWidth = (int)(this.bounds.Width / 2);
        int subHeight = (int)(this.bounds.Height / 2);
        int x = this.bounds.X;
        int y = this.bounds.Y;

        this.nodes[0] = new QuadTree(this.level + 1, new Rectangle(x + subWidth, y, subWidth, subHeight));
        this.nodes[1] = new QuadTree(this.level + 1, new Rectangle(x, y, subWidth, subHeight));
        this.nodes[2] = new QuadTree(this.level + 1, new Rectangle(x, y + subHeight, subWidth, subHeight));
        this.nodes[3] = new QuadTree(this.level + 1, new Rectangle(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    private int GetIndex(Rectangle rect)
    {
        int index = -1;
        double verticalMidpoint = this.bounds.X + (this.bounds.Width / 2f);
        double horizontalMidpoint = this.bounds.Y + (this.bounds.Height / 2f);

        bool topQuad = (rect.X < horizontalMidpoint && rect.Y + rect.Width < horizontalMidpoint);
        bool bottomQuad = (rect.Y > horizontalMidpoint);

        if (rect.X < verticalMidpoint && rect.X + rect.Height < verticalMidpoint)
        {
            if (topQuad)
            {
                index = 1;
            }
            else if (bottomQuad)
            {
                index = 2;
            }
        }
        else if (rect.X > verticalMidpoint)
        {
            if (topQuad)
            {
                index = 0;
            }
            else if (bottomQuad)
            {
                index = 3;
            }
        }

        return index;
    }

    public void Insert(Rectangle physComp)
    {
        if (this.nodes[0] != null!)
        {
            int index = this.GetIndex(physComp);

            if (index != -1)
            {
                this.nodes[index].Insert(physComp);
                return;
            }
        }

        this.objects.Add(physComp);

        if (this.objects.Count <= this.MaxObjects || this.level >= this.MaxLevels)
        {
            return;
        }

        if (this.nodes[0] == null!)
        {
            this.Split();
        }

        int i = 0;
        while (i < this.objects.Count)
        {
            int index = this.GetIndex(this.objects[i]);
            if (index != -1)
            {
                this.nodes[index].Insert(this.objects[i]);
                this.objects.RemoveAt(i);
                continue;
            }

            i++;
        }
    }

    public IEnumerable<Rectangle> Retrieve(Rectangle physics)
    {
        List<Rectangle> returnObjects = new List<Rectangle>();
        int index = this.GetIndex(physics);
        if (index != -1 && this.nodes[0] != null!)
        {
            returnObjects.AddRange(this.nodes[index].Retrieve(physics));
        }

        returnObjects.AddRange(this.objects);

        return returnObjects;
    }
}