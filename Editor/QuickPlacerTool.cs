using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Bu bir Editör Penceresi aracýdýr.
public class QuickPlacerTool : EditorWindow
{
    [Header("Yerleþtirme Ayarlarý (Sol Týk)")]
    public GameObject prefabToPlace;   // Koyulacak obje
    public LayerMask targetLayer;      // Hangi katmanýn üzerine koyulacak? (Örn: Placement)
    public float gridSize = 1.0f;      // Grid boyutu (Genelde 1)
    public float yOffset = 0.5f;       // Yükseklik ayarý

    [Header("Silme Ayarlarý (Sað Týk)")]
    // YENÝ: Sadece bu katmandaki objeler sað týk ile silinebilir.
    // (Yanlýþlýkla zemini silmemek için önemli).
    public LayerMask deleteableLayers;

    [Header("Durum")]
    public bool isPlacementModeActive = false; // Boyama modu açýk mý?

    private GameObject previewObject; // Hayalet önizleme objesi

    [MenuItem("Tools/Hýzlý Yerleþtirici (Grid Snap & Silme)")]
    public static void ShowWindow()
    {
        GetWindow<QuickPlacerTool>("Hýzlý Yerleþtirici");
    }

    void OnGUI()
    {
        GUILayout.Label("Hýzlý Obje Aracý (Sol: Koy / Sað: Sil)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Ayar kutucuklarý
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Koyulacak Prefab:", prefabToPlace, typeof(GameObject), false);
        targetLayer = LayerMaskField("Hedef Zemin Katmaný:", targetLayer);
        gridSize = EditorGUILayout.FloatField("Grid Boyutu:", gridSize);
        yOffset = EditorGUILayout.FloatField("Yükseklik Ofseti:", yOffset);

        GUILayout.Space(10);
        // YENÝ GUI ALANI
        deleteableLayers = LayerMaskField("Silinebilir Katmanlar:", deleteableLayers);
        GUILayout.Space(10);

        // Modu Aç/Kapa Butonu
        GUI.backgroundColor = isPlacementModeActive ? Color.green : Color.red;
        if (GUILayout.Button(isPlacementModeActive ? "Mod: AÇIK (Kapatmak için týkla)" : "Mod: KAPALI (Açmak için týkla)", GUILayout.Height(40)))
        {
            ToggleMode();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Mod AÇIKKEN:\n" +
            "- SOL TIK: Hedef Zemin üzerine prefab koyar.\n" +
            "- SAÐ TIK: Silinebilir Katmanlardaki objeyi siler.\n" +
            "- CTRL + Z: Ýþlemi geri alýr.",
            MessageType.Info);
    }

    void ToggleMode()
    {
        isPlacementModeActive = !isPlacementModeActive;
        if (!isPlacementModeActive && previewObject != null) DestroyImmediate(previewObject);
        SceneView.RepaintAll(); // Deðiþikliði hemen yansýt
    }

    void OnEnable() { SceneView.duringSceneGui += OnSceneGUI; }
    void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; if (previewObject != null) DestroyImmediate(previewObject); }

    // --- SAHNE EKRANINDAKÝ ÝÞLEMLER ---
    void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacementModeActive) return;

        Event e = Event.current;

        // ---------------------------------------------------------
        // 1. SAÐ TIK ÝLE SÝLME ÝÞLEMÝ (Önce bunu kontrol et)
        // ---------------------------------------------------------
        // Button 1 = Sað Týk. Alt tuþuna basýlmýyorsa (Kamera dönüþü deðilse).
        if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
        {
            Ray rayDelete = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hitDelete;

            // Sadece "Silinebilir Katmanlar"a çarpan ýþýnlarý dikkate al
            if (Physics.Raycast(rayDelete, out hitDelete, Mathf.Infinity, deleteableLayers))
            {
                // Geri alýnabilir (Undo) þekilde objeyi yok et
                Undo.DestroyObjectImmediate(hitDelete.collider.gameObject);
                e.Use(); // Olayý tüket (Baþka menüler açýlmasýn)
                return;  // Silme yaptýysak yerleþtirme yapma, çýk.
            }
        }

        // ---------------------------------------------------------
        // 2. SOL TIK ÝLE YERLEÞTÝRME VE ÖNÝZLEME
        // ---------------------------------------------------------
        if (prefabToPlace == null) { if (previewObject != null) DestroyImmediate(previewObject); return; }

        Ray rayPlace = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hitPlace;

        // Sadece "Hedef Zemin Katmaný" üzerine yerleþtirme yap
        if (Physics.Raycast(rayPlace, out hitPlace, Mathf.Infinity, targetLayer))
        {
            // Grid Hesapla
            float x = Mathf.Round(hitPlace.point.x / gridSize) * gridSize;
            float z = Mathf.Round(hitPlace.point.z / gridSize) * gridSize;
            float y = hitPlace.point.y + yOffset;
            Vector3 finalPosition = new Vector3(x, y, z);

            // Önizleme Göster
            if (previewObject == null)
            {
                previewObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
                previewObject.hideFlags = HideFlags.HideAndDontSave;
                foreach (var col in previewObject.GetComponentsInChildren<Collider>()) col.enabled = false;
            }
            previewObject.transform.position = finalPosition;

            // Sol Týk (Button 0) ile Yerleþtir
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                PlaceObject(finalPosition);
                e.Use();
            }
        }
        else
        {
            if (previewObject != null) DestroyImmediate(previewObject);
        }

        if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) sceneView.Repaint();
    }

    void PlaceObject(Vector3 position)
    {
        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
        newObj.transform.position = position;
        GameObject parent = GameObject.Find("Placed_Objects");
        if (parent == null) parent = new GameObject("Placed_Objects");
        newObj.transform.parent = parent.transform;
        Undo.RegisterCreatedObjectUndo(newObj, "Place Object: " + newObj.name);
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