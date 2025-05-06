using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    [Tooltip("Stopnie obrotu skyboxu na sekundê")]
    public float speed = 1f;

    void Update()
    {
        // Oblicz now¹ rotacjê (mod 360, ¿eby utrzymaæ wartoœæ w zakresie 0–360)
        float rot = (Time.time * speed) % 360f;
        // Ustaw parametr "_Rotation" materia³u skyboxu
        RenderSettings.skybox.SetFloat("_Rotation", rot);
        // (opcjonalnie, jeœli u¿ywasz wbudowanego GI)
        DynamicGI.UpdateEnvironment();
    }
}
