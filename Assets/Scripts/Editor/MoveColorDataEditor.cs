using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MoveColorData))]
public class MoveColorDataEditor : Editor
{
    MoveColorData data;

    private void OnEnable()
    {
        data = target as MoveColorData;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (data.sprite == null)
            return;

        Texture2D texture = AssetPreview.GetAssetPreview(data.sprite);
        GUILayout.Label("", GUILayout.Height(data.sprite.rect.height), GUILayout.Width(data.sprite.rect.width));
        GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
    }
}