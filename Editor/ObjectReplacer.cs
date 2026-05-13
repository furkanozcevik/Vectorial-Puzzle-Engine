using UnityEngine;
using UnityEditor; // Editör kütüphanesi

public class ObjectReplacer : EditorWindow
{
    public GameObject replacementPrefab; // Yeni obje (Prefab)
    public bool keepRotation = true;
    public bool keepScale = true;

    [MenuItem("Tools/Obje Deðiþtirici")] // Üst menüde çýkacak isim
    static void Init()
    {
        // Pencereyi aç
        GetWindow(typeof(ObjectReplacer));
    }

    void OnGUI()
    {
        GUILayout.Label("Seçili Objeleri Deðiþtir", EditorStyles.boldLabel);

        // Prefab seçme kutusu
        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Yeni Obje (Prefab):", replacementPrefab, typeof(GameObject), false);

        keepRotation = EditorGUILayout.Toggle("Yönü Koru (Rotation)", keepRotation);
        keepScale = EditorGUILayout.Toggle("Boyutu Koru (Scale)", keepScale);

        if (GUILayout.Button("DEÐÝÞTÝR (Replace)"))
        {
            ReplaceSelectedObjects();
        }
    }

    void ReplaceSelectedObjects()
    {
        if (replacementPrefab == null)
        {
            Debug.LogError("Lütfen önce yeni bir Prefab atayýn!");
            return;
        }

        // Seçili olan her obje için dön
        foreach (GameObject oldObj in Selection.gameObjects)
        {
            // Yeni objeyi oluþtur (Prefab olarak)
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);

            // Konumu kopyala
            newObj.transform.position = oldObj.transform.position;

            // Ýsteðe baðlý özellikleri kopyala
            if (keepRotation) newObj.transform.rotation = oldObj.transform.rotation;
            if (keepScale) newObj.transform.localScale = oldObj.transform.localScale;

            // Hiyerarþideki yerini (Parent) kopyala
            newObj.transform.parent = oldObj.transform.parent;
            newObj.name = replacementPrefab.name; // Ýsmini düzelt

            // Ýþlemi "Geri Alýnabilir" (Undo) yap (Ctrl+Z çalýþsýn diye)
            Undo.RegisterCreatedObjectUndo(newObj, "Replace Object");
            Undo.DestroyObjectImmediate(oldObj);
        }

        Debug.Log("Objeler baþarýyla deðiþtirildi!");
    }
}