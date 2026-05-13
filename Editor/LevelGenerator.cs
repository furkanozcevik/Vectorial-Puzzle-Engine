using UnityEngine;
using UnityEditor;

public class LevelGenerator : EditorWindow
{
    [Header("Harita Görseli")]
    public Texture2D mapImage;

    [Header("Temel Yapý Prefablarý")]
    public GameObject floorPrefab;        // Yeþil (Zemin)
    public GameObject waterPrefab;        // Mavi (Su)
    public GameObject wallPrefab;         // Siyah (Duvar)

    [Header("Dolgu ve Destek Prefablarý")]
    // YENÝ: En alta serilecek dolgu malzemesi (Örn: Anakaya)
    public GameObject subFloorFillerPrefab;
    // Yükseltilerin altýna konacak destek kolonu
    public GameObject supportColumnPrefab;

    // Temel Renk Tanýmlarý
    private Color c_green = new Color(0, 1, 0);
    private Color c_blue = new Color(0, 0, 1);
    private Color c_black = new Color(0, 0, 0);

    [MenuItem("Tools/Sade Level Oluþtur (+Alt Dolgu)")]
    static void Init()
    {
        GetWindow(typeof(LevelGenerator));
    }

    void OnGUI()
    {
        GUILayout.Label("Yapý Oluþturucu (Alt Dolgulu)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        mapImage = (Texture2D)EditorGUILayout.ObjectField("Harita (PNG):", mapImage, typeof(Texture2D), false);

        GUILayout.Space(10);
        GUILayout.Label("Ana Prefablar:", EditorStyles.label);
        floorPrefab = (GameObject)EditorGUILayout.ObjectField("Zemin (Yeþil):", floorPrefab, typeof(GameObject), false);
        waterPrefab = (GameObject)EditorGUILayout.ObjectField("Su (Mavi):", waterPrefab, typeof(GameObject), false);
        wallPrefab = (GameObject)EditorGUILayout.ObjectField("Duvar (Siyah):", wallPrefab, typeof(GameObject), false);

        GUILayout.Space(5);
        GUILayout.Label("Dolgu Prefablarý:", EditorStyles.label);
        // YENÝ GUI ALANI
        subFloorFillerPrefab = (GameObject)EditorGUILayout.ObjectField("En Alt Dolgu (Y=-1.5):", subFloorFillerPrefab, typeof(GameObject), false);
        supportColumnPrefab = (GameObject)EditorGUILayout.ObjectField("Katman Desteði (Support):", supportColumnPrefab, typeof(GameObject), false);

        GUILayout.Space(20);

        if (GUILayout.Button("YAPIYI ÝNÞA ET"))
        {
            GenerateLevel();
        }

        if (GUILayout.Button("TEMÝZLE"))
        {
            ClearScene();
        }
    }

    void GenerateLevel()
    {
        if (mapImage == null) { Debug.LogError("Resim Yok!"); return; }
        if (mapImage.width > 100 || mapImage.height > 100) { Debug.LogError("Resim Çok Büyük!"); return; }

        ClearScene();

        // Hiyerarþi Gruplarý
        GameObject levelParent = new GameObject("Generated_Structure");
        GameObject subFloorGroup = new GameObject("SubFloor_Bedrock"); // Yeni grup
        GameObject wallGroup = new GameObject("Walls");
        GameObject floorGroup = new GameObject("Floors_and_Water");
        GameObject supportGroup = new GameObject("Supports");

        subFloorGroup.transform.parent = levelParent.transform;
        wallGroup.transform.parent = levelParent.transform;
        floorGroup.transform.parent = levelParent.transform;
        supportGroup.transform.parent = levelParent.transform;

        // -------------------------------------------------------------------------
        // ADIM 0: EN ALT DOLGUYU (SUB-FLOOR) YERLEÞTÝR (Y = -1.5)
        // Resmin ne renk olduðuna bakmaksýzýn tüm alaný kaplar.
        // -------------------------------------------------------------------------
        if (subFloorFillerPrefab != null)
        {
            for (int x = 0; x < mapImage.width; x++)
            {
                for (int z = 0; z < mapImage.height; z++)
                {
                    Vector3 bedrockPos = new Vector3(x, -1.5f, z);
                    SpawnObj(subFloorFillerPrefab, bedrockPos, subFloorGroup, "Bedrock");
                }
            }
        }
        // -------------------------------------------------------------------------


        // ADIM 1: RESMÝ OKU VE DÝÐER KATMANLARI ÝNÞA ET
        for (int x = 0; x < mapImage.width; x++)
        {
            for (int z = 0; z < mapImage.height; z++)
            {
                Color pixelColor = mapImage.GetPixel(x, z);

                if (pixelColor.a == 0) continue; // Þeffafsa üstüne bir þey koyma (Ama altýnda bedrock olacak)

                // Duvar Kontrolü
                if (ColorsEqualRGB(pixelColor, c_black))
                {
                    SpawnObj(wallPrefab, new Vector3(x, 0.5f, z), wallGroup, "Wall");
                    continue;
                }

                // Katman Hesaplama
                float maxVal = Mathf.Max(pixelColor.r, Mathf.Max(pixelColor.g, pixelColor.b));
                int layer = 0;
                if (maxVal > 0.8f) layer = 0;
                else if (maxVal > 0.4f) layer = 1;
                else if (maxVal > 0.2f) layer = 2;
                else continue;

                Color baseColor = SnapColor(new Color(pixelColor.r / maxVal, pixelColor.g / maxVal, pixelColor.b / maxVal));

                float yPos = -0.5f + (layer * 1.0f);
                Vector3 pos = new Vector3(x, yPos, z);

                // Katman Destekleri (Supports) - Yükseltilerin altý için
                // (Not: Artýk en altta bedrock olduðu için sadece havada kalan kýsýmlara destek atýyoruz)
                if (layer > 0 && supportColumnPrefab != null)
                {
                    for (int i = 0; i < layer; i++)
                    {
                        float supportY = -0.5f + (i * 1.0f);
                        SpawnObj(supportColumnPrefab, new Vector3(x, supportY, z), supportGroup, "Support");
                    }
                }

                // Zemin veya Su Yerleþtirme
                if (ColorsMatch(baseColor, c_green))
                {
                    SpawnObj(floorPrefab, pos, floorGroup, "Floor");
                }
                else if (ColorsMatch(baseColor, c_blue))
                {
                    SpawnObj(waterPrefab, pos, floorGroup, "Water");
                }
            }
        }
        Debug.Log("Alt Dolgulu Yapý Ýnþa Edildi!");
    }

    void SpawnObj(GameObject prefab, Vector3 pos, GameObject parent, string namePrefix)
    {
        if (prefab != null)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = pos;
            obj.transform.parent = parent.transform;
            obj.name = namePrefix + $"_{pos.x}_{pos.y}_{pos.z}";
        }
    }

    void ClearScene()
    {
        GameObject old = GameObject.Find("Generated_Structure");
        if (old != null) DestroyImmediate(old);
    }

    Color SnapColor(Color c)
    {
        return new Color(c.r > 0.5f ? 1 : 0, c.g > 0.5f ? 1 : 0, c.b > 0.5f ? 1 : 0);
    }

    bool ColorsMatch(Color c1, Color c2)
    {
        return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b;
    }

    bool ColorsEqualRGB(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.1f && Mathf.Abs(c1.g - c2.g) < 0.1f && Mathf.Abs(c1.b - c2.b) < 0.1f;
    }
}