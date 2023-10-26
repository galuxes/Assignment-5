using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
        {   Create(pos + Vector3.left  * halfWidth + Vector3.down * halfWidth + Vector3.back    * halfWidth, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth + Vector3.down * halfWidth + Vector3.back    * halfWidth, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth + Vector3.up   * halfWidth + Vector3.back    * halfWidth, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth + Vector3.up   * halfWidth + Vector3.back    * halfWidth, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth + Vector3.down * halfWidth + Vector3.forward * halfWidth, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth + Vector3.down * halfWidth + Vector3.forward * halfWidth, halfWidth/2, depth-1),
            Create(pos + Vector3.left  * halfWidth + Vector3.up   * halfWidth + Vector3.forward * halfWidth, halfWidth/2, depth-1), 
            Create(pos + Vector3.right * halfWidth + Vector3.up   * halfWidth + Vector3.forward * halfWidth, halfWidth/2, depth-1)});
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
        int i = 0;
        var vecDiff = sphere.position - position;
        if (vecDiff.x > 0) i += 1;
        if (vecDiff.y > 0) i += 2;
        if (vecDiff.z > 0) i += 4;
        children[i].Insert(sphere);
        if (vecDiff.sqrMagnitude < sphere.Radius * sphere.Radius)
        {
            if (vecDiff.x < sphere.Radius) ; //add to whatever index that guy is;
            //same for y
            //same for z
        }
    }

    /// <summary>
    /// Resolves collisions in all children, as only leaf nodes can hold particles.
    /// </summary>
    public void ResolveCollisions()
    {
        for (int i = 0; i < children.Length-1; i++)
        {
            children[i].ResolveCollisions();
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
    private List<Sphere> children;
    
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
        
    }

    /// <summary>
    /// Calls CollisionDetection.ApplyCollisionResolution() on every pair of
    /// spheres in this node.
    /// </summary>
    public void ResolveCollisions()
    {
        for (int i = 0; i < spheres.Length; i++)
        {
            Sphere s1 = spheres[i];
            for (int j = i + 1; j < spheres.Length; j++)
            {
                Sphere s2 = spheres[j];
                ApplyCollisionResolution(s1, s2);
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
