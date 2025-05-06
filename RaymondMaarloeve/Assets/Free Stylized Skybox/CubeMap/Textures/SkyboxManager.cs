using UnityEngine;

public class RotateSkybox : MonoBehaviour
{
    [Tooltip("Stopnie obrotu skyboxu na sekund�")]
    public float speed = 1f;

    void Update()
    {
        // Oblicz now� rotacj� (mod 360, �eby utrzyma� warto�� w zakresie 0�360)
        float rot = (Time.time * speed) % 360f;
        // Ustaw parametr "_Rotation" materia�u skyboxu
        RenderSettings.skybox.SetFloat("_Rotation", rot);
        // (opcjonalnie, je�li u�ywasz wbudowanego GI)
        DynamicGI.UpdateEnvironment();
    }
}
