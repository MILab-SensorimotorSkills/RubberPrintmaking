using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private readonly int maxObjects;
    private readonly int maxLevels;
    private int level;
    private List<Vector3> objects;
    private Rect bounds;
    private QuadTree[] nodes;

    public QuadTree(int level, Rect bounds, int maxObjects = 10, int maxLevels = 5)
    {
        this.level = level;
        this.bounds = bounds;
        this.maxObjects = maxObjects;
        this.maxLevels = maxLevels;
        objects = new List<Vector3>();
        nodes = new QuadTree[4];
    }

    public void Clear()
    {
        objects.Clear();
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    private void Split()
    {
        float subWidth = bounds.width / 2f;
        float subHeight = bounds.height / 2f;
        float x = bounds.x;
        float y = bounds.y;

        nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));
        nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    private int GetIndex(Rect pRect)
    {
        int index = -1;
        double verticalMidpoint = bounds.x + (bounds.width / 2);
        double horizontalMidpoint = bounds.y + (bounds.height / 2);

        bool topQuadrant = (pRect.y < horizontalMidpoint && pRect.y + pRect.height < horizontalMidpoint);
        bool bottomQuadrant = (pRect.y > horizontalMidpoint);

        if (pRect.x < verticalMidpoint && pRect.x + pRect.width < verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 1;
            }
            else if (bottomQuadrant)
            {
                index = 2;
            }
        }
        else if (pRect.x > verticalMidpoint)
        {
            if (topQuadrant)
            {
                index = 0;
            }
            else if (bottomQuadrant)
            {
                index = 3;
            }
        }

        return index;
    }

    public void Insert(Vector3 pRect)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(new Rect(pRect.x, pRect.z, 0, 0));

            if (index != -1)
            {
                nodes[index].Insert(pRect);

                return;
            }
        }

        objects.Add(pRect);

        if (objects.Count > maxObjects && level < maxLevels)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(new Rect(objects[i].x, objects[i].z, 0, 0));
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public List<Vector3> Retrieve(List<Vector3> returnObjects, Rect pRect)
    {
        int index = GetIndex(pRect);
        if (index != -1 && nodes[0] != null)
        {
            nodes[index].Retrieve(returnObjects, pRect);
        }

        returnObjects.AddRange(objects);

        return returnObjects;
    }
}
