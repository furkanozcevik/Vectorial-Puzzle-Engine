using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ClickToReplace : EditorWindow
{
    [Header("Deðiþtirme Ayarlarý")]
    public GameObject replacementPrefab; // Yerine geçecek yeni obje
    public LayerMask targetLayer;        // Sadece bu katmandakileri deðiþtir (Güvenlik için)
    public bool keepRotation = true;     // Yönü korunsun mu?
    public bool keepScale = true;        // Boyut korunsun mu?

    [Header("Durum")]
    public bool isModeActive = false;

    [MenuItem("Tools/Týkla ve Deðiþtir (Click Replacer)")]
    static void Init()
    {
        GetWindow(typeof(ClickToReplace));
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnGUI()
    {
        GUILayout.Label("Týkla ve Deðiþtir Aracý", EditorStyles.boldLabel);
        GUILayout.Space(10);

        replacementPrefab = (GameObject)EditorGUILayout.ObjectField("Yeni Prefab:", replacementPrefab, typeof(GameObject), false);
        targetLayer = LayerMaskField("Hedef Katman (Filtre):", targetLayer);

        keepRotation = EditorGUILayout.Toggle("Yönü Koru (Rotation):", keepRotation);
        keepScale = EditorGUILayout.Toggle("Boyutu Koru (Scale):", keepScale);

        GUILayout.Space(20);

        // Aktiflik Butonu (Renkli)
        GUI.backgroundColor = isModeActive ? Color.green : Color.red;
        if (GUILayout.Button(isModeActive ? "MOD AÇIK (Kapatmak için Týkla)" : "MOD KAPALI (Açmak için Týkla)", GUILayout.Height(40)))
        {
            isModeActive = !isModeActive;
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        if (isModeActive)
        {
            EditorGUILayout.HelpBox("Sahne ekranýnda deðiþtirmek istediðin objeye SOL TIKLA. \nDikkat: Ýþlem kalýcýdýr (Ctrl+Z ile geri alýnabilir).", MessageType.Warning);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!isModeActive || replacementPrefab == null) return;

        // Unity'nin varsayýlan seçim karesini engelle (Týklayýnca objeyi seçmesin, deðiþtirsin)
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        Event e = Event.current;

        // Sadece Sol Týk ve Alt tuþuna basýlmýyorsa (Kamera dönüþü deðilse)
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            // Fiziksel bir objeye çarptýk mý?
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
            {
                GameObject targetObj = hit.collider.gameObject;

                // Kendisiyle deðiþtirmeyi engelle
                // (Prefab adý "(Clone)" içerebilir, bu basit bir kontrol)
                if (PrefabUtility.GetCorrespondingObjectFromSource(targetObj) == replacementPrefab)
                {
                    Debug.Log("Zaten ayný obje.");
                    return;
                }

                ReplaceObject(targetObj);

                // Olayý tüket (Unity baþka iþlem yapmasýn)
                e.Use();
            }
        }
    }

    void ReplaceObject(GameObject oldObj)
    {
        // Yeni objeyi Prefab baðlantýsýný koruyarak oluþtur
        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab);

        // Pozisyonu kopyala
        newObj.transform.position = oldObj.transform.position;

        // Rotasyonu kopyala (Ýsteðe baðlý)
        if (keepRotation) newObj.transform.rotation = oldObj.transform.rotation;
        else newObj.transform.rotation = replacementPrefab.transform.rotation;

        // Scale kopyala (Ýsteðe baðlý)
        if (keepScale) newObj.transform.localScale = oldObj.transform.localScale;
        else newObj.transform.localScale = replacementPrefab.transform.localScale;

        // Hiyerarþi düzenini koru (Ayný klasörün/parent'ýn içine koy)
        newObj.transform.parent = oldObj.transform.parent;

        // Ýsmi düzelt (Eski ismin aynýsý olmasýn, prefab ismi olsun)
        newObj.name = replacementPrefab.name;

        // UNDO SÝSTEMÝ (Çok Önemli)
        // Bu sayede Ctrl + Z yapýnca eski obje geri gelir.
        Undo.RegisterCreatedObjectUndo(newObj, "Replace Object");
        Undo.DestroyObjectImmediate(oldObj);

        Debug.Log(oldObj.name + " -> " + newObj.name + " ile deðiþtirildi.");
    }

    // LayerMask GUI Yardýmcýsý
    LayerMask LayerMaskField(string label, LayerMask layerMask)
    {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();
        for (int i = 0; i < 32; i++) { string layerName = LayerMask.LayerToName(i); if (layerName != "") { layers.Add(layerName); layerNumbers.Add(i); } }
        int maskWithoutEmpty = 0; for (int i = 0; i < layerNumbers.Count; i++) { if (((1 << layerNumbers[i]) & layerMask.value) > 0) maskWithoutEmpty |= (1 << i); }
        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
        int mask = 0; for (int i = 0; i < layerNumbers.Count; i++) { if ((maskWithoutEmpty & (1 << i)) > 0) mask |= (1 << layerNumbers[i]); }
        layerMask.value = mask; return layerMask;
    }
}