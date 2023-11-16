using System.Collections.Generic;
using UnityEngine;

public class MeshMergeManager : MonoBehaviour
{
    public List<MeshFilter> meshFilters;
    public List<MeshRenderer> meshRenderers;
    [SerializeField] private MeshRenderer combinedMeshRenderer;
    [SerializeField] private MeshFilter combinedMeshFilter;

    public void MergeMeshes()
    {
        // Create arrays to hold meshes and materials
        Mesh[] meshes = new Mesh[meshFilters.Count];
        Material[] materials = new Material[1];

        materials[0] = meshRenderers[0].sharedMaterial;
        
        // Populate arrays
        for (int i = 0; i < meshFilters.Count; i++)
        {
            meshes[i] = meshFilters[i].sharedMesh;
           
        }

        // Create a new combined mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(GetCombineInstances(meshFilters.ToArray()), true, true);

        // Set the combined mesh to the parent GameObject
        combinedMeshFilter.sharedMesh = combinedMesh;

        // Set the materials for the combined mesh
        combinedMeshRenderer.materials = materials;

        // Remove individual mesh renderers
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = false;
        }
    }

    CombineInstance[] GetCombineInstances(MeshFilter[] meshFilters)
    {
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        return combineInstances;
    }
}