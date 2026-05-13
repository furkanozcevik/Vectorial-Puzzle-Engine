using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class ModularLevelGen : EditorWindow
{
    [System.Serializable]
    public class SegmentData
    {
        public GameObject prefab;
        [Tooltip("Tiklenirse: Bir sonraki par�a 1 birim yukar� konur.")]
        public bool raisesNextLevel;
    }

    [Header("LevelParts")]
    public List<SegmentData> startSegments = new List<SegmentData>();  // 0-6 aras�
    public List<SegmentData> middleSegments = new List<SegmentData>(); // 7-13 aras�
    public List<SegmentData> endSegments = new List<SegmentData>();    // 14-20 aras�

    [Header("Dolgu ve Duvar")]
    public GameObject fillerPrefab;
    public GameObject borderWallPrefab;

    [Header("Sabit Ayarlar")]
    public int chunkLength = 7; // Her par�an�n uzunlu�u
    public int mapWidth = 21;   // 0'dan 20'ye (Toplam 21 kare)
    public string saveFolderPath = "Assets/GeneratedLevels_Final";

    [MenuItem("Tools/LevelGenerator")]
    static void Init()
    {
        GetWindow(typeof(ModularLevelGen));
    }

    void OnGUI()
    {
        GUILayout.Label("", EditorStyles.boldLabel);
        GUILayout.Space(10);

        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);

        EditorGUILayout.PropertyField(so.FindProperty("startSegments"), true);
        EditorGUILayout.PropertyField(so.FindProperty("middleSegments"), true);
        EditorGUILayout.PropertyField(so.FindProperty("endSegments"), true);

        GUILayout.Space(5);
        SerializedProperty fillerProp = so.FindProperty("fillerPrefab");
        EditorGUILayout.PropertyField(fillerProp);
        SerializedProperty wallProp = so.FindProperty("borderWallPrefab");
        EditorGUILayout.PropertyField(wallProp);

        so.ApplyModifiedProperties();

        GUILayout.Space(20);

        if (GUILayout.Button("Create All Combinations"))
        {
            GenerateAllCombinations();
        }
    }

    void GenerateAllCombinations()
    {
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
            AssetDatabase.Refresh();
        }

        int currentCount = 0;
        int totalCount = startSegments.Count * middleSegments.Count * endSegments.Count;

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // 3'l� Kombinasyon (Giri� -> Orta -> Son)
        for (int i = 0; i < startSegments.Count; i++)
        {
            for (int j = 0; j < middleSegments.Count; j++)
            {
                for (int k = 0; k < endSegments.Count; k++)
                {
                    currentCount++;
                    float progress = (float)currentCount / totalCount;
                    EditorUtility.DisplayProgressBar("B�l�m �n�a Ediliyor...", $"Level {currentCount}/{totalCount}", progress);

                    CreateFixedLevel(startSegments[i], middleSegments[j], endSegments[k], currentCount);
                }
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log("T�m Leveller 0-7-14 Koordinatlar�na G�re Olu�turuldu!");
    }

    void CreateFixedLevel(SegmentData start, SegmentData mid, SegmentData end, int index)
    {
        UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        float currentY = 0;

        // --- 1. G�R�� PAR�ASI (Z = 0) ---
        // Aral�k: 0'dan 6'ya kadar
        SpawnSegment(start.prefab, 0, currentY);
        if (start.raisesNextLevel) currentY += 1; // Y�kselme kontrol�

        // --- 2. ORTA PAR�A (Z = 7) ---
        // Aral�k: 7'den 13'e kadar
        SpawnSegment(mid.prefab, 7, currentY);
        SpawnFillers(7, currentY); // Alt�n� doldur
        if (mid.raisesNextLevel) currentY += 1; // Y�kselme kontrol�

        // --- 3. SON PAR�A (Z = 14) ---
        // Aral�k: 14'ten 20'ye kadar
        SpawnSegment(end.prefab, 14, currentY);
        SpawnFillers(14, currentY); // Alt�n� doldur

        // --- SINIRLARI KAPAT ---
        // Toplam uzunluk 21 birim
        CreateBorders(21, currentY + 5);

        // KAYDET
        string sceneName = $"Level_{index}_Aligned.unity";
        string fullPath = Path.Combine(saveFolderPath, sceneName);
        EditorSceneManager.SaveScene(newScene, fullPath);
        AddSceneToBuildSettings(fullPath);
    }

    void SpawnSegment(GameObject prefab, float z, float y)
    {
        if (prefab != null)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.rotation = Quaternion.identity; // Rotasyonu s�f�rla
            obj.transform.position = new Vector3(0, y, z);
        }
    }

    void SpawnFillers(float z, float currentY)
    {
        if (fillerPrefab == null) return;

        // Havada duran par�alar�n alt�n� doldur
        for (float y = currentY - 1; y >= 0; y--)
        {
            GameObject filler = (GameObject)PrefabUtility.InstantiatePrefab(fillerPrefab);
            filler.transform.rotation = Quaternion.identity;
            filler.transform.position = new Vector3(0, y, z);
            filler.name = "Filler_Y" + y;
        }
    }

    void CreateBorders(float totalLength, float height)
    {
        if (borderWallPrefab == null) return;

        // Harita 0-20 aras� (Geni�lik 21)
        // Merkez X = 10, Merkez Z = 10.5
        float centerX = 10f;
        float centerZ = 10.5f;
        float centerY = height / 2f;

        // Sol Duvar (X = -1)
        SpawnWall(new Vector3(-1, centerY, centerZ), new Vector3(1, height, totalLength));

        // Sa� Duvar (X = 21)
        SpawnWall(new Vector3(21, centerY, centerZ), new Vector3(1, height, totalLength));

        // Arka Duvar (Z = -1)
        SpawnWall(new Vector3(centerX, centerY, -1), new Vector3(23, height, 1)); // Geni�li�i biraz art�rd�k

        // �n Duvar (Z = 21)
        SpawnWall(new Vector3(centerX, centerY, 21), new Vector3(23, height, 1));
    }

    void SpawnWall(Vector3 pos, Vector3 scale)
    {
        GameObject wall = (GameObject)PrefabUtility.InstantiatePrefab(borderWallPrefab);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        wall.name = "BorderWall";

        MeshRenderer mr = wall.GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false; // G�r�nmez yap
    }

    void AddSceneToBuildSettings(string path)
    {
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[originalScenes.Length + 1];
        System.Array.Copy(originalScenes, newScenes, originalScenes.Length);
        newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = newScenes;
    }
}