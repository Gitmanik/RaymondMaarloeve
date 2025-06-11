using UnityEngine;

/// <summary>
/// Converts all direct children of this GameObject from SkinnedMeshRenderers to static meshes
/// with MeshFilters, MeshRenderers, and MeshColliders attached.
/// </summary>
public class SmartMeshColliderAdder : MonoBehaviour
{
    /// <summary>
    /// Converts each child GameObject containing a SkinnedMeshRenderer into a static mesh setup.
    /// This includes copying mesh and material, destroying the SkinnedMeshRenderer,
    /// and attaching a MeshFilter, MeshRenderer, and MeshCollider.
    /// </summary>
    [ContextMenu("Convert SkinnedMesh to static + assign Mesh from name or SMR")]
    void ProcessChildren()
    {
        foreach (Transform child in transform)
        {
            string meshName = child.name;

            Mesh selectedMesh = null;
            Material selectedMaterial = null;

            // Step 1: Try to extract mesh and material from SkinnedMeshRenderer
            var smr = child.GetComponent<SkinnedMeshRenderer>();
            if (smr)
            {
                selectedMesh = smr.sharedMesh;
                selectedMaterial = smr.sharedMaterial;
                DestroyImmediate(smr);
            }

            // Step 2: Ensure a MeshFilter exists
            var mf = child.GetComponent<MeshFilter>();
            if (!mf)
                mf = child.gameObject.AddComponent<MeshFilter>();

            // Step 3: Assign mesh from SMR or (optional) name
            if (selectedMesh != null)
            {
                mf.sharedMesh = selectedMesh;
            }

            // Step 4: Ensure a MeshRenderer exists and assign material
            var mr = child.GetComponent<MeshRenderer>();
            if (!mr)
                mr = child.gameObject.AddComponent<MeshRenderer>();

            if (selectedMaterial != null)
                mr.sharedMaterial = selectedMaterial;

            // Step 5: Add MeshCollider if missing, using the selected mesh
            if (!child.GetComponent<MeshCollider>())
            {
                var mc = child.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = selectedMesh;
                mc.convex = false;
            }
        }

        Debug.Log("All children processed.");
    }

    /// <summary>
    /// Searches all loaded Mesh assets in the project for one with a given name.
    /// </summary>
    /// <param name="name">The name of the mesh to find.</param>
    /// <returns>The matching Mesh if found, otherwise null.</returns>
    Mesh FindMeshByName(string name)
    {
        Mesh[] allMeshes = Resources.FindObjectsOfTypeAll<Mesh>();
        foreach (var mesh in allMeshes)
        {
            if (mesh.name == name)
                return mesh;
        }
        return null;
    }
}
