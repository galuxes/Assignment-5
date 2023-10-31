using System.Collections;
using System.Collections.Generic;
using static CollisionDetection;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollisionManager : MonoBehaviour
{
    Octree tree;
    private List<Sphere> spheres = new List<Sphere>();

    public enum CollisionType
    {
        Standard,
        Octree
    }

    public static CollisionType collisionType = CollisionType.Standard;

    [SerializeField]
    public uint nStartingParticles = 100;

    [SerializeField]
    private GameObject particlePrefab;

    private void Start()
    {
        tree = Octree.Create(Vector3.zero, 5);
        
        
        for (int i = 0; i < nStartingParticles; i++)
        {
            var sphereOBJ = Instantiate(particlePrefab);
            sphereOBJ.transform.position = Vector3.right * Random.Range(-4f,4f) + Vector3.up * Random.Range(-4f,4f) + Vector3.forward * Random.Range(-4f,4f);
            var sphere = sphereOBJ.GetComponent<Sphere>();
            sphere.velocity = Vector3.right * Random.Range(-2f, 2f) + Vector3.up * Random.Range(-2f, 2f) + Vector3.forward * Random.Range(-2f, 2f);
            spheres.Add(sphere);
            tree.Insert(sphere);
        }
        // Create the Octree. Create prefabs within the bounding box of the scene.
    }

    private void TreeCollisionResolution()
    {
        tree.ResolveCollisions();
        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        for (int i = 0; i < spheres.Count; i++)
        {
            Sphere s1 = spheres[i];
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }
        }
        // Perform sphere-sphere collisions using the Octree
    }

    private void StandardCollisionResolution()
    {
        PlaneCollider[] planes = FindObjectsOfType<PlaneCollider>();
        for (int i = 0; i < spheres.Count; i++)
        {
            Sphere s1 = spheres[i];
            for (int j = i + 1; j < spheres.Count; j++)
            {
                Sphere s2 = spheres[j];
                ApplyCollisionResolution(s1, s2);
            }
            foreach (PlaneCollider plane in planes)
            {
                ApplyCollisionResolution(s1, plane);
            }
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks = 0;

        if (collisionType == CollisionType.Standard)
        {
            StandardCollisionResolution();
        }
        else
        {
            TreeCollisionResolution();
        }
        // Call correct collision resolution type based
        // on collisionType variable.
    }

    private void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            collisionType = collisionType == CollisionType.Standard ? CollisionType.Octree : CollisionType.Standard;
        }
        // Switch collision types if the "C" key is pressed.
    }
}
