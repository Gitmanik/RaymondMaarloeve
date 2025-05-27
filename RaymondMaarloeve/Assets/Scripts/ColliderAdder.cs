using UnityEngine;

public class SmartMeshColliderAdder : MonoBehaviour
{
    [ContextMenu("Zamień SkinnedMesh na statyczny + przypisz Mesh po nazwie lub z SMR")]
    void ProcessChildren()
    {
        foreach (Transform child in transform)
        {
            string meshName = child.name;

            Mesh selectedMesh = null;
            Material selectedMaterial = null;

            // === 1. Jeśli jest SkinnedMeshRenderer, zapamiętaj mesh i materiał
            var smr = child.GetComponent<SkinnedMeshRenderer>();
            if (smr)
            {
                selectedMesh = smr.sharedMesh;
                selectedMaterial = smr.sharedMaterial;

                DestroyImmediate(smr);
            }

            // === 2. Dodaj MeshFilter jeśli nie istnieje
            var mf = child.GetComponent<MeshFilter>();
            if (!mf)
                mf = child.gameObject.AddComponent<MeshFilter>();

            // === 3. Przypisz mesh (najpierw z SMR, potem po nazwie)
            if (selectedMesh != null)
            {
                mf.sharedMesh = selectedMesh;
            }

            // === 4. Dodaj MeshRenderer i przypisz materiał
            var mr = child.GetComponent<MeshRenderer>();
            if (!mr)
                mr = child.gameObject.AddComponent<MeshRenderer>();

            if (selectedMaterial != null)
                mr.sharedMaterial = selectedMaterial;

            // === 5. Dodaj MeshCollider (jeśli nie istnieje)
            if (!child.GetComponent<MeshCollider>())
            {
                var mc = child.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = selectedMesh;
                mc.convex = false;
            }
        }

        Debug.Log("✅ Wszystkie dzieci przetworzone.");
    }

    // Szuka mesha po nazwie w folderze projektu
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
