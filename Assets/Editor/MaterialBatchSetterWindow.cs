// Editor/MaterialBatchSetterWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialBatchSetterWindow : EditorWindow
{
    private Transform rootParent;
    private Material materialToApply;
    private List<Transform> excludedBranches = new List<Transform>();
    private bool includeInactive = true;
    private Vector2 scroll;

    [MenuItem("Tools/Material Batch Setter")]
    public static void ShowWindow()
    {
        GetWindow<MaterialBatchSetterWindow>("Material Batch Setter");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Batch Material Setter", EditorStyles.boldLabel);

        rootParent = (Transform)EditorGUILayout.ObjectField("Root Parent", rootParent, typeof(Transform), true);
        materialToApply = (Material)EditorGUILayout.ObjectField("Material To Apply", materialToApply, typeof(Material), false);
        includeInactive = EditorGUILayout.Toggle("Include Inactive Objects", includeInactive);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Excluded Branches (object + all its children skipped)");

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(120));
        for (int i = 0; i < excludedBranches.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            excludedBranches[i] = (Transform)EditorGUILayout.ObjectField(excludedBranches[i], typeof(Transform), true);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                excludedBranches.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Add Exclusion Slot"))
            excludedBranches.Add(null);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(rootParent == null || materialToApply == null))
        {
            if (GUILayout.Button("Apply Material To All (Except Excluded)"))
            {
                ApplyMaterial();
            }
        }
    }

    private void ApplyMaterial()
    {
        HashSet<Transform> excludedSet = new HashSet<Transform>();
        foreach (var t in excludedBranches)
            if (t != null) excludedSet.Add(t);

        List<Renderer> targets = new List<Renderer>();
        CollectRenderers(rootParent, excludedSet, targets);

        if (targets.Count == 0)
        {
            Debug.LogWarning("No renderers found to modify.");
            return;
        }

        Undo.RecordObjects(targets.ToArray(), "Batch Set Material");

        foreach (var r in targets)
        {
            Material[] mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = materialToApply;
            r.sharedMaterials = mats;
            EditorUtility.SetDirty(r);
        }

        Debug.Log($"Applied material to {targets.Count} renderer(s).");
    }

    private void CollectRenderers(Transform current, HashSet<Transform> excluded, List<Renderer> results)
    {
        if (excluded.Contains(current)) return; // skip this whole branch
        if (!includeInactive && !current.gameObject.activeSelf) return;

        Renderer r = current.GetComponent<Renderer>();
        if (r != null) results.Add(r);

        foreach (Transform child in current)
            CollectRenderers(child, excluded, results);
    }
}