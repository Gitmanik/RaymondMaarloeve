using UnityEngine;

/// <summary>
/// Converts all child objects of the current GameObject that use SkinnedMeshRenderer
/// into static objects with MeshFilter, MeshRenderer, and MeshCollider components.
/// </summary>
public class SmartMeshColliderAdder : MonoBehaviour
{
    /// <summary>
    /// Converts skinned meshes to static meshes and assigns colliders.
    /// Trigger this via context menu in the Unity Inspector.
    /// </summary>
    [ContextMenu("Convert SkinnedMesh to static with Mesh + assign by name or SMR")]
    void ProcessChildren()
    {
        foreach (Transform child in transform)
        {
            string meshName = child.name;
            Mesh selectedMesh = null;
            Material selectedMaterial = null;

            // === STEP 1: Extract mesh and material from SkinnedMeshRenderer, if present ===
            var smr = child.GetComponent<SkinnedMeshRenderer>();
            if (smr)
            {
                selectedMesh = smr.sharedMesh;
                selectedMaterial = smr.sharedMaterial;

                DestroyImmediate(smr);
                Debug.Log($"Removed SkinnedMeshRenderer from: {child.name}");
            }

            // === STEP 2: Add MeshFilter if missing ===
            var mf = child.GetComponent<MeshFilter>();
            if (!mf)
            {
                mf = child.gameObject.AddComponent<MeshFilter>();
                Debug.Log($"Added MeshFilter to: {child.name}");
            }

            // === STEP 3: Assign mesh from SkinnedMeshRenderer or by name ===
            if (selectedMesh != null)
            {
                mf.sharedMesh = selectedMesh;
                Debug.Log($"Assigned mesh from SkinnedMeshRenderer to: {child.name}");
            }
            else
            {
                selectedMesh = FindMeshByName(meshName);
                if (selectedMesh != null)
                {
                    mf.sharedMesh = selectedMesh;
                    Debug.Log($"Assigned mesh by name ({meshName}) to: {child.name}");
                }
                else
                {
                    Debug.LogWarning($"No mesh found for: {child.name}");
                }
            }

            // === STEP 4: Add MeshRenderer and assign material ===
            var mr = child.GetComponent<MeshRenderer>();
            if (!mr)
            {
                mr = child.gameObject.AddComponent<MeshRenderer>();
                Debug.Log($"Added MeshRenderer to: {child.name}");
            }

            if (selectedMaterial != null)
            {
                mr.sharedMaterial = selectedMaterial;
                Debug.Log($"Assigned material from SkinnedMeshRenderer to: {child.name}");
            }

            // === STEP 5: Add MeshCollider if not present ===
            if (!child.GetComponent<MeshCollider>())
            {
                var mc = child.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = selectedMesh;
                mc.convex = false;
                Debug.Log($"Added MeshCollider to: {child.name}");
            }
        }

        Debug.Log("All child objects processed.");
    }

    /// <summary>
    /// Finds a mesh by name in loaded resources.
    /// </summary>
    /// <param name="name">Name of the mesh to search for.</param>
    /// <returns>Mesh if found; otherwise, null.</returns>
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
