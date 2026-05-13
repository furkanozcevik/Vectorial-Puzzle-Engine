using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabReplacer : EditorWindow
{
    [Header("Deðiþim Ayarlarý")]
    public GameObject sourcePrefab; // Sahnede aranacak olan (Eski)
    public GameObject targetPrefab; // Yerine konulacak olan (Yeni)

    [Header("Seçenekler")]
    public bool keepRotation = true; // Yönü koru
    public bool keepScale = true;    // Boyutu koru
    public bool keepParent = true;   // Hiyerarþideki yerini koru
    public bool keepName = false;    // Ýsmi koru (False ise yeni prefabýn adýný alýr)

    [MenuItem("Tools/Toplu Prefab Deðiþtirici")]
    static void Init()
    {
        GetWindow(typeof(PrefabReplacer));
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Toplu Deðiþtirme Aracý", EditorStyles.boldLabel);
        GUILayout.Space(10);

        sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Eski Prefab (Aranacak):", sourcePrefab, typeof(GameObject), false);
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Yeni Prefab (Yerine Gelecek):", targetPrefab, typeof(GameObject), false);

        GUILayout.Space(10);

        keepRotation = EditorGUILayout.Toggle("Rotasyonu Koru:", keepRotation);
        keepScale = EditorGUILayout.Toggle("Boyutu (Scale) Koru:", keepScale);
        keepParent = EditorGUILayout.Toggle("Hiyerarþiyi (Parent) Koru:", keepParent);
        keepName = EditorGUILayout.Toggle("Eski Ýsmi Koru:", keepName);

        GUILayout.Space(20);

        if (GUILayout.Button("DEÐÝÞTÝR (Replace All)"))
        {
            ReplacePrefabs();
        }
    }

    void ReplacePrefabs()
    {
        if (sourcePrefab == null || targetPrefab == null)
        {
            Debug.LogError("Lütfen hem Eski hem de Yeni prefabý seçin!");
            return;
        }

        // Sahnedeki tüm objeleri tara
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> objectsToReplace = new List<GameObject>();

        // Sadece bizim aradýðýmýz prefaba baðlý olanlarý listele
        foreach (GameObject obj in allObjects)
        {
            // Bu obje bir prefab mý ve kaynaðý bizim sourcePrefab mý?
            if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == sourcePrefab)
            {
                objectsToReplace.Add(obj);
            }
        }

        if (objectsToReplace.Count == 0)
        {
            Debug.LogWarning("Sahnede bu prefaba ait obje bulunamadý!");
            return;
        }

        // Deðiþtirme Döngüsü
        foreach (GameObject oldObj in objectsToReplace)
        {
            // Yeni objeyi oluþtur (Prefab baðlantýsýný koruyarak)
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab);

            // Transform verilerini aktar
            newObj.transform.position = oldObj.transform.position;

            if (keepRotation) newObj.transform.rotation = oldObj.transform.rotation;
            else newObj.transform.rotation = targetPrefab.transform.rotation;

            if (keepScale) newObj.transform.localScale = oldObj.transform.localScale;
            else newObj.transform.localScale = targetPrefab.transform.localScale;

            // Hiyerarþi
            if (keepParent) newObj.transform.parent = oldObj.transform.parent;

            // Ýsim
            if (keepName) newObj.name = oldObj.name;

            // Undo (Geri Alma) Desteði
            Undo.RegisterCreatedObjectUndo(newObj, "Replace Prefab");
            Undo.DestroyObjectImmediate(oldObj);
        }

        Debug.Log($"{objectsToReplace.Count} adet obje baþarýyla deðiþtirildi!");
    }
}