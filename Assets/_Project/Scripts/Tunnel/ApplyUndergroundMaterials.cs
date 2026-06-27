using UnityEngine;

public class ApplyUndergroundMaterials : MonoBehaviour
{
    [Header("Underground Materials")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material ceilingMaterial;
    [SerializeField] private Material woodSupportMaterial;

    [Header("Material Settings")]
    [Range(0f, 1f)] public float roughness = 0.85f;
    [Range(0f, 1f)] public float metallic = 0.05f;
    [Range(0f, 1f)] public float occlusionStrength = 0.9f;
    [Range(0f, 0.1f)] public float parallaxHeight = 0.035f;

    private void Start()
    {
        ApplyMaterialsToChildren();
    }

    public void ApplyMaterialsToChildren()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.name.Contains("Floor") ||
                renderer.gameObject.name.Contains("floor") ||
                renderer.gameObject.name.Contains("Ground"))
            {
                ApplyMaterial(renderer, floorMaterial);
            }
            else if (renderer.gameObject.name.Contains("Wall") ||
                     renderer.gameObject.name.Contains("wall") ||
                     renderer.gameObject.name.Contains("Side"))
            {
                ApplyMaterial(renderer, wallMaterial);
            }
            else if (renderer.gameObject.name.Contains("Ceiling") ||
                     renderer.gameObject.name.Contains("ceiling") ||
                     renderer.gameObject.name.Contains("Roof") ||
                     renderer.gameObject.name.Contains("Top"))
            {
                ApplyMaterial(renderer, ceilingMaterial);
            }
            else if (renderer.gameObject.name.Contains("Support") ||
                     renderer.gameObject.name.Contains("Beam") ||
                     renderer.gameObject.name.Contains("Wood") ||
                     renderer.gameObject.name.Contains("Timber"))
            {
                ApplyMaterial(renderer, woodSupportMaterial);
            }
            else
            {
                // Default to wall material for unknown surfaces
                ApplyMaterial(renderer, wallMaterial);
            }
        }
    }

    private void ApplyMaterial(Renderer renderer, Material material)
    {
        if (material != null)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = new Material(material);
                ApplyMaterialProperties(mats[i]);
            }
            renderer.materials = mats;
        }
    }

    private void ApplyMaterialProperties(Material mat)
    {
        if (mat.HasProperty("_Smoothness"))
        {
            mat.SetFloat("_Smoothness", 1f - roughness);
        }

        if (mat.HasProperty("_Metallic"))
        {
            mat.SetFloat("_Metallic", metallic);
        }

        if (mat.HasProperty("_OcclusionStrength"))
        {
            mat.SetFloat("_OcclusionStrength", occlusionStrength);
        }

        if (mat.HasProperty("_Parallax"))
        {
            mat.SetFloat("_Parallax", parallaxHeight);
        }
    }

    public void AddBumpVariation(Renderer renderer, float bumpScale = 0.5f)
    {
        if (renderer != null)
        {
            Material[] mats = renderer.materials;
            foreach (var mat in mats)
            {
                if (mat.HasProperty("_BumpScale"))
                {
                    mat.SetFloat("_BumpScale", bumpScale * Random.Range(0.7f, 1.3f));
                }
            }
        }
    }

    public void AddMineralStains(Renderer renderer, Color stainColor, float intensity = 0.3f)
    {
        if (renderer != null)
        {
            Material[] mats = renderer.materials;
            foreach (var mat in mats)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", stainColor * intensity);
                }
            }
        }
    }
}
