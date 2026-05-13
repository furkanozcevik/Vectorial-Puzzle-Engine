using UnityEngine;
using UnityEditor;

public class LinearFiller : EditorWindow
{
    public Transform corner1;     // first corner
    public Transform corner2;     // second corner
    public GameObject fillPrefab; // fill Object
    public float spacing = 1.0f;  // space 

    // YENÝ: Rotasyon Ayarý
    public Vector3 fixedRotation = Vector3.zero; // Sabit açý (0,0,0) veya (90,0,0) vb.
    public bool usePrefabRotation = true; // Prefabýn kendi açýsýný mý kullansýn?

    [MenuItem("Tools/Dikdörtgen Alan Doldur")]
    static void Init()
    {
        GetWindow(typeof(LinearFiller));
    }

    void OnGUI()
    {
        GUILayout.Label("Filler", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Corners"))
        {
            if (Selection.transforms.Length == 2)
            {
                corner1 = Selection.transforms[0];
                corner2 = Selection.transforms[1];
            }
            else
            {
                Debug.LogWarning("select the object");
            }
        }
        GUILayout.Space(5);

        corner1 = (Transform)EditorGUILayout.ObjectField("corner 1:", corner1, typeof(Transform), true);
        corner2 = (Transform)EditorGUILayout.ObjectField("corner 2:", corner2, typeof(Transform), true);
        fillPrefab = (GameObject)EditorGUILayout.ObjectField("filler Prefab:", fillPrefab, typeof(GameObject), false);

        spacing = EditorGUILayout.FloatField("Spacing:", spacing);

        GUILayout.Space(5);
        usePrefabRotation = EditorGUILayout.Toggle("Save Prefab Rotations", usePrefabRotation);

        if (!usePrefabRotation)
        {
            fixedRotation = EditorGUILayout.Vector3Field("Özel Rotasyon:", fixedRotation);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fill the Area"))
        {
            FillRectangle();
        }
    }

    void FillRectangle()
    {
        if (corner1 == null || corner2 == null || fillPrefab == null)
        {
            Debug.LogError("Eksik bilgi! Lütfen Köþeleri ve Prefabý atayýn.");
            return;
        }

        // 1. Koordinat Sýnýrlarýný Bul (Min/Max Hesapla)
        // Hangi objenin saðda veya solda olduðunu bilmediðimiz için Min/Max kullanýyoruz.
        float minX = Mathf.Min(corner1.position.x, corner2.position.x);
        float maxX = Mathf.Max(corner1.position.x, corner2.position.x);

        float minZ = Mathf.Min(corner1.position.z, corner2.position.z);
        float maxZ = Mathf.Max(corner1.position.z, corner2.position.z);

        // Yüksekliði (Y) ilk köþeden alýyoruz
        float yPos = corner1.position.y;

        // Rotasyonu belirle
        Quaternion spawnRot = usePrefabRotation ? fillPrefab.transform.rotation : Quaternion.Euler(fixedRotation);

        int count = 0;

        // 2. ÝÇ ÝÇE DÖNGÜ (X ve Z ekseninde tarama)
        // Küçük tolerans (0.01f) ekliyoruz ki floating point hatasýndan son kareyi atlamasýn.
        for (float x = minX; x <= maxX + 0.01f; x += spacing)
        {
            for (float z = minZ; z <= maxZ + 0.01f; z += spacing)
            {
                Vector3 spawnPos = new Vector3(x, yPos, z);

                // Prefab oluþtur (Baðlantýlý)
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(fillPrefab);
                newObj.transform.position = spawnPos;
                newObj.transform.rotation = spawnRot; // SABÝT ROTASYON

                // Düzenli dursun diye Köþe 1'in içine atalým
                newObj.transform.parent = corner1.parent;
                newObj.name = fillPrefab.name;

                Undo.RegisterCreatedObjectUndo(newObj, "Fill Rect");
                count++;
            }
        }

        Debug.Log(count + " adet obje dikdörtgen alana yerleþtirildi!");
    }
}