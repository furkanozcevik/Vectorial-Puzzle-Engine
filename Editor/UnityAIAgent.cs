using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class UnityAIArchitect : EditorWindow
{
    // --- AYARLAR ---
    private string apiKey = "";
    private string userPrompt = "Örn: Kırmızı bir küp oluştur, içine 'Rotator' adında dönme scripti yaz ve ekle.";
    private string statusMessage = "Hazır.";
    private Vector2 scrollPos;

    // API Key'i hafızada tutmak için key
    private const string PREFS_KEY_NAME = "GeminiAPIKey_V1";

    [MenuItem("Tools/Unity AI Architect (Fixed)")]
    public static void ShowWindow()
    {
        GetWindow<UnityAIArchitect>("AI Architect");
    }

    private void OnEnable()
    {
        // Pencere açıldığında kayıtlı anahtarı yükle
        if (EditorPrefs.HasKey(PREFS_KEY_NAME))
            apiKey = EditorPrefs.GetString(PREFS_KEY_NAME);
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity AI Architect (v2.0)", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // API Key Alanı
        GUILayout.BeginHorizontal();
        GUILayout.Label("API Key:", GUILayout.Width(60));
        string newKey = EditorGUILayout.TextField(apiKey);
        if (newKey != apiKey)
        {
            apiKey = newKey;
            EditorPrefs.SetString(PREFS_KEY_NAME, apiKey); // Değişince kaydet
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Ne Yapmamı İstersin?", EditorStyles.label);
        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.Height(80));

        GUILayout.Space(10);

        GUI.backgroundColor = new Color(0f, 0.8f, 0.4f);
        if (GUILayout.Button("İnşa Et / Kodla", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                statusMessage = "HATA: Lütfen API Key giriniz.";
                return;
            }
            ProcessRequest();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        GUILayout.Label("Konsol:", EditorStyles.boldLabel);

        // Konsol Görünümü
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.richText = true;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
        EditorGUILayout.TextArea(statusMessage, style);
        EditorGUILayout.EndScrollView();
    }

    // --- YAPAY ZEKA İLETİŞİMİ ---

    private async void ProcessRequest()
    {
        statusMessage = "Gemini ile iletişim kuruluyor...";

        // 1. SİSTEM TALİMATI (Prompt Engineering)
        string systemInstruction = @"
        Sen Unity Editor içinde çalışan uzman bir geliştiricisin. Görevin, kullanıcı isteğini Unity API komutlarına çevirmektir.
        
        SADECE aşağıdaki JSON formatında yanıt ver. Başka hiçbir metin veya markdown (```json) kullanma.
        
        Kullanabileceğin Eylemler (actions):
        - 'create_script': Yeni C# scripti oluşturur. (name: Sınıf Adı, content: Kodun tamamı)
        - 'create_primitive': Basit obje oluşturur. (name: Obje Adı, content: Cube, Sphere, Plane, Capsule)
        - 'create_material': Materyal oluşturur. (name: Materyal Adı, content: Hex Renk Kodu örn: #FF0000)
        - 'instantiate_prefab': Klasördeki prefabı sahnede oluşturur. (name: Obje Adı, content: Prefab Dosya Adı)
        
        ÖRNEK JSON ÇIKTISI:
        {
            ""steps"": [
                { ""action"": ""create_primitive"", ""name"": ""PlayerBase"", ""content"": ""Cube"" },
                { ""action"": ""create_material"", ""name"": ""RedMat"", ""content"": ""#FF0000"" },
                { ""action"": ""create_script"", ""name"": ""Mover"", ""content"": ""using UnityEngine; public class Mover : MonoBehaviour { void Update() { transform.Translate(Vector3.forward * Time.deltaTime); } }"" }
            ]
        }
        ";

        string fullJsonBody = ConstructJsonBody(systemInstruction + "\nKULLANICI: " + userPrompt);

        // 2. HTTP İSTEĞİ (Gemini 1.5 Flash - En Hızlı ve Kararlı URL)
        string url = $"[https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=](https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=){apiKey}";

        // ProcessRequest fonksiyonunun içindeki using bloğunu bununla değiştir:

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(fullJsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // BU SATIRI EKLE (Güvenlik sertifikası kontrolünü atlar - Sadece test için):
            request.certificateHandler = new BypassCertificate();

            var operation = request.SendWebRequest();
            // ... geri kalan kod aynı
        }
    }
    // --- JSON İŞLEME VE UYGULAMA ---

    private void ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            string cleanJson = ExtractJsonClean(jsonResponse);
            AIPlan plan = JsonUtility.FromJson<AIPlan>(cleanJson);

            if (plan != null && plan.steps != null)
            {
                statusMessage = $"Plan Alındı. {plan.steps.Count} adım uygulanıyor...\n";
                ExecutePlan(plan.steps);
            }
            else
            {
                statusMessage += "\nJSON formatı anlaşılamadı.";
                Debug.LogError("Ham Yanıt: " + jsonResponse);
            }
        }
        catch (Exception e)
        {
            statusMessage = "Parse Hatası: " + e.Message;
            Debug.LogError(jsonResponse);
        }
    }

    private async void ExecutePlan(List<AIAction> actions)
    {
        foreach (var step in actions)
        {
            statusMessage += $"> {step.action}: {step.name}\n";

            switch (step.action)
            {
                case "create_primitive":
                    CreatePrimitive(step.name, step.content);
                    break;
                case "create_material":
                    CreateMaterial(step.name, step.content);
                    break;
                case "create_script":
                    CreateScript(step.name, step.content);
                    // Script oluşturunca Unity compile eder, bu biraz karışık bir durumdur.
                    // Kullanıcıya bilgi veriyoruz.
                    statusMessage += "   (Script oluşturuldu, Unity derleyince eklenebilir)\n";
                    await Task.Delay(500);
                    AssetDatabase.Refresh();
                    break;
                case "instantiate_prefab":
                    InstantiatePrefab(step.name, step.content);
                    break;
            }
            await Task.Delay(100); // Unity UI donmasın diye nefes aldır
        }
        statusMessage += "\n✅ İŞLEM TAMAMLANDI.";
    }

    // --- EYLEM FONKSİYONLARI ---

    void CreatePrimitive(string name, string type)
    {
        GameObject go = null;
        type = type.ToLower();
        if (type.Contains("cube")) go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        else if (type.Contains("sphere")) go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        else if (type.Contains("plane")) go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        else if (type.Contains("capsule")) go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        else go = new GameObject(name);

        if (go != null)
        {
            go.name = name;
            // Sıfır noktasına koy
            go.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(go, "AI Create Primitive");
        }
    }

    void CreateMaterial(string name, string hexColor)
    {
        // URP veya Built-in uyumlu basit shader
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            mat.color = color;
        }

        string path = $"Assets/{name}.mat";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(mat, path);
        EditorGUIUtility.PingObject(mat);
    }

    void CreateScript(string className, string content)
    {
        string path = $"Assets/{className}.cs";
        if (File.Exists(path))
        {
            statusMessage += "   UYARI: Script zaten var, üzerine yazıldı.\n";
        }

        // Markdown temizliği (Bazen ```csharp yazar)
        content = content.Replace("```csharp", "").Replace("```", "").Trim();

        File.WriteAllText(path, content);
    }

    void InstantiatePrefab(string objName, string prefabName)
    {
        // Projede prefab ara
        string[] guids = AssetDatabase.FindAssets(prefabName + " t:Prefab");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = objName;
                instance.transform.position = Vector3.zero;
                Undo.RegisterCreatedObjectUndo(instance, "AI Prefab");
            }
        }
        else
        {
            statusMessage += $"   UYARI: Prefab bulunamadı ({prefabName})\n";
        }
    }

    // --- YARDIMCI SINIFLAR VE METODLAR ---

    // Gemini yanıtındaki karmaşık JSON'ı temizler
    private string ExtractJsonClean(string raw)
    {
        // Regex ile ilk { ve son } arasını al
        var match = Regex.Match(raw, @"\{[\s\S]*\}");
        return match.Success ? match.Value : "{}";
    }

    // JSON Gövdesi Oluşturucu
    private string ConstructJsonBody(string prompt)
    {
        // Basit string birleştirme yerine JsonUtility kullanabilirdik ama
        // iç içe yapıları manuel kurmak daha güvenli (Unity JsonUtility limiti yüzünden)
        string escapedPrompt = prompt.Replace("\"", "\\\"").Replace("\n", "\\n");
        return $@"
        {{
            ""contents"": [{{
                ""parts"": [{{ ""text"": ""{escapedPrompt}"" }}]
            }}]
        }}";
    }

    [Serializable]
    public class AIPlan
    {
        public List<AIAction> steps;
    }

    [Serializable]
    public class AIAction
    {
        public string action;
        public string name;
        public string content;
    }
    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // Her sertifikayı kabul et
            return true;
        }
    }
}