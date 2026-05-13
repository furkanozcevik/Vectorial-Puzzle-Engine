using UnityEngine;
using UnityEditor;

public class ObjectPlacerTool : EditorWindow
{
    [Header("Harita Görseli")]
    public Texture2D mapImage;

    [Header("Sadece Bu Objeler Eklenecek")]
    public GameObject gunPrefab;      // Silah
    public GameObject targetPrefab;   // Hedef
    public GameObject mirrorPrefab;   // Ayna
    public GameObject splitterPrefab; // Bölücü
    public GameObject elevatorPrefab; // Asansör
    public GameObject portalPrefab;   // Portal

    [Header("Renk Kodlarý (Haritadaki Karþýlýklar)")]
    // Not: Zemin ve Duvar renklerini tanýmlamýyoruz çünkü onlarý pas geçeceðiz.
    public Color gunColor = new Color(0, 1, 1);       // Cyan
    public Color targetColor = Color.red;             // Kýrmýzý
    public Color mirrorColor = new Color(0.5f, 0, 1); // Mor (Ayna)
    public Color splitterColor = Color.green;         // Yeþil (Bölücü)
    public Color elevatorColor = Color.yellow;        // Sarý
    public Color portalColor = Color.magenta;         // Magenta

    [Header("Yerleþim Ayarlarý")]
    public float objectHeight = 0.5f; // Objelerin yerden yüksekliði (Zemin üstüne oturmasý için)

    [MenuItem("Tools/Haritaya Göre Obje Yerleþtir (Sadece Logic)")]
    static void Init()
    {
        GetWindow(typeof(ObjectPlacerTool));
    }

    void OnGUI()
    {
        GUILayout.Label("Obje Yerleþtirici (Zemin/Duvar Hariç)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        mapImage = (Texture2D)EditorGUILayout.ObjectField("Plan Resmi (PNG):", mapImage, typeof(Texture2D), false);

        GUILayout.Space(10);
        GUILayout.Label("Prefablar:", EditorStyles.label);

        gunPrefab = (GameObject)EditorGUILayout.ObjectField("Silah (Cyan):", gunPrefab, typeof(GameObject), false);
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Hedef (Kýrmýzý):", targetPrefab, typeof(GameObject), false);
        mirrorPrefab = (GameObject)EditorGUILayout.ObjectField("Ayna (Mor):", mirrorPrefab, typeof(GameObject), false);
        splitterPrefab = (GameObject)EditorGUILayout.ObjectField("Bölücü (Yeþil):", splitterPrefab, typeof(GameObject), false);
        elevatorPrefab = (GameObject)EditorGUILayout.ObjectField("Asansör (Sarý):", elevatorPrefab, typeof(GameObject), false);
        portalPrefab = (GameObject)EditorGUILayout.ObjectField("Portal (Magenta):", portalPrefab, typeof(GameObject), false);

        GUILayout.Space(10);
        objectHeight = EditorGUILayout.FloatField("Yükseklik (Y):", objectHeight);

        GUILayout.Space(20);

        if (GUILayout.Button("OBJELERÝ YERLEÞTÝR"))
        {
            PlaceObjects();
        }

        if (GUILayout.Button("SADECE OBJELERÝ TEMÝZLE"))
        {
            ClearObjects();
        }
    }

    void PlaceObjects()
    {
        if (mapImage == null)
        {
            Debug.LogError("Lütfen bir harita resmi seçin!");
            return;
        }

        // Önceki objeleri temizle (Üst üste binmesin)
        ClearObjects();

        // Objeler için bir klasör oluþtur
        GameObject objectsParent = new GameObject("Level_Logic_Objects");

        for (int x = 0; x < mapImage.width; x++)
        {
            for (int z = 0; z < mapImage.height; z++)
            {
                Color pixelColor = mapImage.GetPixel(x, z);

                // Þeffaf, Siyah (Duvar) veya Beyaz/Yeþil (Zemin) ise ATLAMA yapýyoruz.
                // Sadece özel renkleri arýyoruz.

                Vector3 pos = new Vector3(x, objectHeight, z);

                if (ColorsEqual(pixelColor, gunColor))
                {
                    Spawn(gunPrefab, pos, objectsParent, "Gun");
                }
                else if (ColorsEqual(pixelColor, targetColor))
                {
                    Spawn(targetPrefab, pos, objectsParent, "Target");
                }
                else if (ColorsEqual(pixelColor, mirrorColor))
                {
                    Spawn(mirrorPrefab, pos, objectsParent, "Mirror");
                }
                else if (ColorsEqual(pixelColor, splitterColor))
                {
                    Spawn(splitterPrefab, pos, objectsParent, "Splitter");
                }
                else if (ColorsEqual(pixelColor, elevatorColor))
                {
                    // Asansör bazen zemine gömülü (0) bazen üstte (0.5) olabilir.
                    Spawn(elevatorPrefab, pos, objectsParent, "Elevator");
                }
                else if (ColorsEqual(pixelColor, portalColor))
                {
                    Spawn(portalPrefab, pos, objectsParent, "Portal");
                }
            }
        }
        Debug.Log("Objeler mevcut haritanýn üzerine yerleþtirildi!");
    }

    void Spawn(GameObject prefab, Vector3 pos, GameObject parent, string namePrefix)
    {
        if (prefab != null)
        {
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            obj.transform.position = pos;
            obj.transform.parent = parent.transform;
            obj.name = namePrefix + $"_{pos.x}_{pos.z}";
        }
    }

    void ClearObjects()
    {
        // Sadece bu scriptin oluþturduðu "Level_Logic_Objects" grubunu siler.
        // Sahnedeki duvarlara ve zeminlere DOKUNMAZ.
        GameObject oldGroup = GameObject.Find("Level_Logic_Objects");
        if (oldGroup != null) DestroyImmediate(oldGroup);
    }

    // Renk Karþýlaþtýrma (Toleranslý)
    bool ColorsEqual(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.1f &&
               Mathf.Abs(c1.g - c2.g) < 0.1f &&
               Mathf.Abs(c1.b - c2.b) < 0.1f;
    }
}