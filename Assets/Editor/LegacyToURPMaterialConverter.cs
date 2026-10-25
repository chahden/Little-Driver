using UnityEditor;
using UnityEngine;

// Converts materials that use Built-in "Legacy Shaders/..." (e.g. Legacy Shaders/Diffuse)
// to URP Simple Lit, keeping their texture, tiling/offset and color.
// Select materials (or a folder of materials) in the Project window, then run
// Tools > Convert Selected Legacy Materials to URP.
public static class LegacyToURPMaterialConverter
{
    const string MenuPath = "Tools/Convert Selected Legacy Materials to URP";

    [MenuItem(MenuPath)]
    static void ConvertSelected()
    {
        Shader simpleLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (simpleLit == null)
        {
            Debug.LogError("URP Simple Lit shader not found. Is the Universal Render Pipeline package installed?");
            return;
        }

        int converted = 0;
        foreach (Material material in Selection.GetFiltered<Material>(SelectionMode.DeepAssets))
        {
            if (!material.shader.name.StartsWith("Legacy Shaders/"))
                continue;

            bool hasMainTex = material.HasProperty("_MainTex");
            Texture mainTex = hasMainTex ? material.GetTexture("_MainTex") : null;
            Vector2 tiling = hasMainTex ? material.GetTextureScale("_MainTex") : Vector2.one;
            Vector2 offset = hasMainTex ? material.GetTextureOffset("_MainTex") : Vector2.zero;
            Color color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;

            Undo.RecordObject(material, "Convert Legacy Material to URP");
            material.shader = simpleLit;
            material.SetTexture("_BaseMap", mainTex);
            material.SetTextureScale("_BaseMap", tiling);
            material.SetTextureOffset("_BaseMap", offset);
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);

            Debug.Log($"Converted {material.name} to URP Simple Lit", material);
            converted++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Converted {converted} legacy material(s) to URP Simple Lit.");
    }

    [MenuItem(MenuPath, true)]
    static bool CanConvertSelected()
    {
        return Selection.GetFiltered<Material>(SelectionMode.DeepAssets).Length > 0;
    }
}
