using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Bu script sadece Unity Editöründe çalýþýr, oyuna dahil edilmez.
public class ObjectReplacerTool : EditorWindow
{
    [System.Serializable]
    public class ReplacementRule
    {
        public string targetName;    // Sahnede aranacak isim (Örn: "Cube_Wall")
        public GameObject newPrefab; // Yerine konacak Prefab
    }

    public List<ReplacementRule> rules = new List<ReplacementRule>();
    public bool keepRotation = true;
    public bool keepScale = true;

    [MenuItem("Tools/Sahne Obje Deðiþtirici (Replacer)")]
    static void Init()
    {
        GetWindow(typeof(ObjectReplacerTool));
    }

    void OnGUI()
    {
        GUILayout.Label("Toplu Obje Deðiþtirici", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Listeyi Editörde Göster
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty stringsProperty = so.FindProperty("rules");
        EditorGUILayout.PropertyField(stringsProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        keepRotation = EditorGUILayout.Toggle("Rotasyonu Koru", keepRotation);
        keepScale = EditorGUILayout.Toggle("Boyutu Koru", keepScale);

        GUILayout.Space(20);

        if (GUILayout.Button("DEÐÝÞTÝR (Tüm Sahne)"))
        {
            ReplaceObjects();
        }
    }

    void ReplaceObjects()
    {
        if (rules.Count == 0) return;

        int count = 0;
        // Sahnedeki tüm objeleri bul (Yavaþ ama editörde sorun olmaz)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // Kurallarý kontrol et
            foreach (ReplacementRule rule in rules)
            {
                if (rule.newPrefab == null) continue;

                // Ýsmi içeriyor mu? (Örn: "Cube (1)", "Cube" içerir)
                if (obj.name.Contains(rule.targetName))
                {
                    // Yeni objeyi oluþtur (Prefab baðlantýlý)
                    GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(rule.newPrefab);

                    // Pozisyonu aktar
                    newObj.transform.position = obj.transform.position;

                    // Rotasyon
                    if (keepRotation) newObj.transform.rotation = obj.transform.rotation;

                    // Scale
                    if (keepScale) newObj.transform.localScale = obj.transform.localScale;

                    // Hiyerarþi (Parent)
                    newObj.transform.parent = obj.transform.parent;

                    // Undo (Ctrl+Z) desteði için kaydet ve eskiyi sil
                    Undo.RegisterCreatedObjectUndo(newObj, "Replace Object");
                    Undo.DestroyObjectImmediate(obj);

                    count++;
                    break; // Bir kural uyduysa diðerlerine bakma
                }
            }
        }

        Debug.Log($"{count} adet obje baþarýyla deðiþtirildi!");
    }
}