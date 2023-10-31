using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using System;

public interface Octree
{
    /// <summary>
    /// Inserts a particle into the octree, descending its children as needed.
    /// </summary>
    /// <param name="particle"></param>
    public void Insert(Sphere particle);

    /// <summary>
    /// Does all necessary collision detection tests.
    /// </summary>
    public void ResolveCollisions();

    /// <summary>
    /// Removes all objects from the Octree.
    /// </summary>
    public void Clear();

    /// <summary>
    /// Creates a new Octree, properly creating children.
    /// </summary>
    /// <param name="pos">The position of this Octree</param>
    /// <param name="halfWidth">The width of this Octree node, from the center to one edge (only needs to be used to calculate children's positions)</param>
    /// <param name="depth">The number of levels beneath this one to create (i.e., depth = 1 means create one node with 8 children. depth = 0 means create only this node. depth = 2 means create one node with 8 children, each of which are Octree's with depth 1.</param>
    /// <returns>The newly created Octree</returns>
    public static Octree Create(Vector3 pos, float halfWidth = 1f, uint depth = 1)
    {
        if (depth == 0)
        {
            return new OctreeObjects();
        }

        return new OctreeNode(pos, new []
        {   Create(pos + Vector3.left  * halfWidth/2 + Vector3.down * halfWidth/2 + Vector3.back    * halfWidth/2, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth/2 + Vector3.down * halfWidth/2 + Vector3.back    * halfWidth/2, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth/2 + Vector3.up   * halfWidth/2 + Vector3.back    * halfWidth/2, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth/2 + Vector3.up   * halfWidth/2 + Vector3.back    * halfWidth/2, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth/2 + Vector3.down * halfWidth/2 + Vector3.forward * halfWidth/2, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth/2 + Vector3.down * halfWidth/2 + Vector3.forward * halfWidth/2, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth/2 + Vector3.up   * halfWidth/2 + Vector3.forward * halfWidth/2, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth/2 + Vector3.up   * halfWidth/2 + Vector3.forward * halfWidth/2, halfWidth/2, depth-1)});
    }
}

/// <summary>
/// An octree that holds 8 children, all of which are Octree's.
/// </summary>
public class OctreeNode : Octree
{
    public Vector3 position;
    public Octree[] children;

    public OctreeNode()
    {
        position = Vector3.zero;
        children = new OctreeObjects[8];
    }
    public OctreeNode(Vector3 position, Octree[] children)
    {
        this.position = position;
        this.children = children;
    }

    /// <summary>
    /// Inserts the given particle into the appropriate children. The particle
    /// may need to be inserted into more than one child.
    /// </summary>
    /// <param name="sphere">The bounding sphere of the particle to insert.</param>
    public void Insert(Sphere sphere)
    {
        var vecDiff = sphere.position - position;
        bool xPos = vecDiff.x > 0;
        bool yPos = vecDiff.y > 0;
        bool zPos = vecDiff.z > 0;
        
        if(vecDiff is { x: <= 0, y: <= 0, z: <= 0 }) children[0].Insert(sphere);
        if(vecDiff is { x: >= 0, y: <= 0, z: <= 0 }) children[1].Insert(sphere);
        if(vecDiff is { x: <= 0, y: >= 0, z: <= 0 }) children[2].Insert(sphere);
        if(vecDiff is { x: >= 0, y: >= 0, z: <= 0 }) children[3].Insert(sphere);
        if(vecDiff is { x: <= 0, y: <= 0, z: >= 0 }) children[4].Insert(sphere);
        if(vecDiff is { x: >= 0, y: <= 0, z: >= 0 }) children[5].Insert(sphere);
        if(vecDiff is { x: <= 0, y: >= 0, z: >= 0 }) children[6].Insert(sphere);
        if(vecDiff is { x: >= 0, y: >= 0, z: >= 0 }) children[7].Insert(sphere);


        /*int i = 0;
        var vecDiff = sphere.position - position;
        if (vecDiff.x >= 0) i += 1;
        if (vecDiff.y >= 0) i += 2;
        if (vecDiff.z >= 0) i += 4;
        children[i].Insert(sphere);
        if (vecDiff.sqrMagnitude < sphere.Radius * sphere.Radius)
        {
            if (Mathf.Abs(vecDiff.x) < sphere.Radius)
            {
                if (vecDiff.x >= 0)
                {
                    children[i-1].Insert(sphere);
                }
                else
                {
                    children[i+1].Insert(sphere);
                }
            }

            if (Mathf.Abs(vecDiff.y) < sphere.Radius)
            {
                if (vecDiff.y >= 0)
                {
                    children[i-2].Insert(sphere);
                }
                else
                {
                    children[i+2].Insert(sphere);
                }
            }

            if (Mathf.Abs(vecDiff.z) < sphere.Radius)
            {
                if (vecDiff.z >= 0)
                {
                    children[i-4].Insert(sphere);
                }
                else
                {
                    children[i+4].Insert(sphere);
                }
            }
        }*/
    }

    /// <summary>
    /// Resolves collisions in all children, as only leaf nodes can hold particles.
    /// </summary>
    public void ResolveCollisions()
    {
        foreach (var child in children)
        {
           child.ResolveCollisions();
        }
    }

    /// <summary>
    /// Removes all particles in each child.
    /// </summary>
    public void Clear()
    {
        foreach (var obj in children)
        {
            obj.Clear();
        }
    }
}

/// <summary>
/// An octree that holds only particles.
/// </summary>
public class OctreeObjects : Octree
{
    private List<Sphere> children = new List<Sphere>();
    
    public ICollection<Sphere> Objects
    {
        get
        {
            return children;
        }
    }

    public OctreeObjects()
    {
        
    }

    /// <summary>
    /// Inserts the particle into this node. It will be compared with all other
    /// particles in this node in ResolveCollisions().
    /// </summary>
    /// <param name="particle">The particle to insert.</param>
    public void Insert(Sphere particle)
    {
        children.Add(particle);
    }

    /// <summary>
    /// Calls CollisionDetection.ApplyCollisionResolution() on every pair of
    /// spheres in this node.
    /// </summary>
    public void ResolveCollisions()
    {
        foreach (var s1 in children)
        {
            foreach (var s2 in children)
            {
                if(s1 != s2) ApplyCollisionResolution(s1, s2);
            }
        }
    }

    /// <summary>
    /// Removes all objects from this node.
    /// </summary>
    public void Clear()
    {
        Objects.Clear();
    }
}
