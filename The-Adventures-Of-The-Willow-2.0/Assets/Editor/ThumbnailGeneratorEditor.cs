using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabThumbnailGenerator))]
public class ThumbnailGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PrefabThumbnailGenerator generator = (PrefabThumbnailGenerator)target;

        if (GUILayout.Button("Generate Thumbnail"))
        {
            generator.GenerateThumbnail();
        }
    }
}



