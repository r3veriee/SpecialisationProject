using UnityEngine;
using UnityEditor;

public class FixNegativeScaleEditor
{
    [MenuItem("Tools/Fix Negative Scales in Scene")]
    public static void FixAllScales()
    {
        // Find every Transform in the active scene, including inactive objects
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        int count = 0;

        foreach (Transform t in allTransforms)
        {
            // Only target valid hierarchy objects in the active scene
            if (t != null && t.gameObject.scene.isLoaded && !EditorUtility.IsPersistent(t.transform.root.gameObject))
            {
                Vector3 currentScale = t.localScale;

                // Check if any axis is negative
                if (currentScale.x < 0 || currentScale.y < 0 || currentScale.z < 0)
                {
                    // Allow Undo functionality in the editor
                    Undo.RecordObject(t, "Fix Negative Scale");

                    // Convert to absolute positive values
                    t.localScale = new Vector3(
                        Mathf.Abs(currentScale.x),
                        Mathf.Abs(currentScale.y),
                        Mathf.Abs(currentScale.z)
                    );

                    // Mark scene as dirty so changes can be saved
                    EditorUtility.SetDirty(t);
                    count++;
                }
            }
        }

        Debug.Log($"Successfully fixed negative scales on {count} GameObjects!");
    }
}
