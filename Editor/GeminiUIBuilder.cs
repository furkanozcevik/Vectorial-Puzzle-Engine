using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public class GeminiUIBuilder : EditorWindow
{
    // --- AYARLAR ---
    private string apiKey = "BURAYA_GEMINI_API_KEY_YAZIN";
    private string userPrompt = "Ana menü oluþtur, içine Oyna, Ayarlar ve Çýkýþ butonu ekle.";

    [System.Serializable]
    public struct UIModel
    {
        public string typeName;     // Örn: "button", "panel", "text", "slider"
        public GameObject prefab;   // Unity Prefabý
    }

    public List<UIModel> availableModels = new List<UIModel>();

    // --- GEMINI JSON YANIT YAPISI ---
    [Serializable]
    public class UIElementData
    {
        public string type;       // "button"
        public string name;       // "PlayButton"
        public string parentName; // "MainMenuPanel" (Hiyerarþi için)
    }

    [Serializable]
    public class GeminiResponseWrapper
    {
        public List<UIElementData> items;
    }

    // --- GEMINI API ÝSTEK YAPISI ---
    [Serializable]
    private class GeminiRequestBody
    {
        public Content[] contents;
    }
    [Serializable] private class Content { public Part[] parts; }
    [Serializable] private class Part { public string text; }


    [MenuItem("Tools/Gemini UI Builder")]
    public static void ShowWindow()
    {
        GetWindow<GeminiUIBuilder>("Gemini UI");
    }

    void OnGUI()
    {
        GUILayout.Label("Gemini Destekli UI Oluþturucu", EditorStyles.boldLabel);

        GUILayout.Space(5);
        apiKey = EditorGUILayout.TextField("API Key:", apiKey);

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Prefab Tipleri (Gemini bunlarý kullanacak):", MessageType.Info);

        // Listeyi çizmek için SerializedObject hilesi
        ScriptableObject target = this;
        SerializedObject so = new SerializedObject(target);
        SerializedProperty listProp = so.FindProperty("availableModels");
        EditorGUILayout.PropertyField(listProp, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        GUILayout.Label("Ýsteðinizi Yazýn:", EditorStyles.label);
        userPrompt = EditorGUILayout.TextArea(userPrompt, GUILayout.Height(60));

        GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
        if (GUILayout.Button("Gemini'ye Sor ve Oluþtur", GUILayout.Height(40)))
        {
            if (string.IsNullOrEmpty(apiKey)) { Debug.LogError("API Key girilmedi!"); return; }
            CallGeminiAPI();
        }
    }

    private async void CallGeminiAPI()
    {
        Debug.Log("Gemini düþünüyör...");

        // 1. Mevcut Prefab Tiplerini Listele (Prompt'a ekleyeceðiz)
        string typesList = "";
        foreach (var m in availableModels) typesList += m.typeName + ", ";

        // 2. System Prompt (Gemini'ye nasýl davranacaðýný öðretiyoruz)
        string systemInstruction = $@"
        Sen bir Unity UI kurucususun. Kullanýcýnýn isteðini analiz et ve bir JSON döndür.
        Kullanabileceðin UI tipleri sadece þunlardýr: [{typesList}].
        Eðer kullanýcý listede olmayan bir þey isterse, listedeki en yakýn tipi seç.
        
        Çýktý formatý ÞU ÞEKÝLDE OLMALIDIR (Sadece JSON, markdown yok):
        {{
            ""items"": [
                {{ ""type"": ""panel"", ""name"": ""MainPanel"", ""parentName"": ""root"" }},
                {{ ""type"": ""button"", ""name"": ""PlayBtn"", ""parentName"": ""MainPanel"" }}
            ]
        }}
        Not: En üstteki kapsayýcýnýn parentName deðeri 'root' olmalý. Alt elemanlarýn parentName deðeri, ebeveynlerinin 'name' deðeri olmalý.
        ";

        string fullPrompt = systemInstruction + "\nKULLANICI ÝSTEÐÝ: " + userPrompt;

        // 3. JSON Ýsteði Hazýrla
        string jsonBody = $@"
        {{
            ""contents"": [{{
                ""parts"": [{{ ""text"": {JsonUtility.ToJson(fullPrompt)} }}]
            }}]
        }}";

        // 4. Web Ýsteði Gönder (Unity 6 / 2022+ uyumlu)
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ParseAndBuildUI(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Gemini Hatasý: " + request.error + "\n" + request.downloadHandler.text);
            }
        }
    }

    private void ParseAndBuildUI(string jsonResponse)
    {
        // Gemini bazen JSON'ý ```json ... ``` bloklarý içine alýr, onlarý temizleyelim.
        // Yanýtýn içindeki asýl metni ayýklamamýz lazým (Google API yapýsý gereði).
        // Basit string parsing ile "text" kýsmýný alýyoruz.

        try
        {
            // API yanýtý karmaþýktýr, basitçe içinden bizim istediðimiz JSON bloðunu bulalým.
            // Not: Prodüksiyonda tam bir JSON parser kullanmak daha iyidir ama Editor için bu yeterli.
            string cleanJson = ExtractJsonContent(jsonResponse);

            GeminiResponseWrapper uiData = JsonUtility.FromJson<GeminiResponseWrapper>(cleanJson);

            if (uiData != null && uiData.items != null)
            {
                BuildScene(uiData.items);
            }
            else
            {
                Debug.LogError("JSON ayrýþtýrýlamadý veya boþ.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Parse Hatasý: " + e.Message);
        }
    }

    // Google API yanýtýndan asýl içeriði çýkaran yardýmcý fonksiyon
    private string ExtractJsonContent(string rawApiJson)
    {
        // Çok basit bir split mantýðý (Geliþmiþ projelerde Newtonsoft.Json kullanýlmalý)
        // Gemini yanýtý þöyledir: { candidates: [ { content: { parts: [ { text: "BÝZÝM JSON" } ] } } ] }
        int startIndex = rawApiJson.IndexOf("items");
        if (startIndex == -1) return "{}";

        // Geriye doðru ilk süslü parantezi bul
        int jsonStart = rawApiJson.LastIndexOf("{", startIndex);

        // Ýleriye doðru son süslü parantezi bul (kabaca)
        // Burada basitlik adýna JSON formatýný temizlemeye çalýþýyoruz
        // Daha temiz yöntem: Sadece süslü parantezler arasýný al.
        string textPart = rawApiJson.Substring(jsonStart);

        // Markdown temizliði
        textPart = textPart.Replace("```json", "").Replace("```", "");

        // Sondaki fazlalýklarý at (API parantezleri vs) - Basit bir hack
        int lastBracket = textPart.LastIndexOf("}");
        textPart = textPart.Substring(0, lastBracket + 1);

        return textPart;
    }

    private void BuildScene(List<UIElementData> items)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject c = new GameObject("Main Canvas");
            canvas = c.AddComponent<Canvas>();
            c.AddComponent<CanvasScaler>();
            c.AddComponent<GraphicRaycaster>();
            c.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Objeleri geçici olarak hafýzada tut (Parent atamasý için)
        Dictionary<string, Transform> createdObjects = new Dictionary<string, Transform>();
        createdObjects["root"] = canvas.transform;

        // Önce hepsini oluþtur
        foreach (var item in items)
        {
            UIModel model = availableModels.Find(x => x.typeName == item.type);
            if (model.prefab != null)
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(model.prefab);
                newObj.name = item.name;

                // Unity UI için RectTransform sýfýrlama
                RectTransform rt = newObj.GetComponent<RectTransform>();
                if (rt) rt.anchoredPosition = Vector2.zero;

                createdObjects[item.name] = newObj.transform;
                Undo.RegisterCreatedObjectUndo(newObj, "Gemini UI Create");
            }
        }

        // Þimdi hiyerarþiyi kur (Parenting)
        foreach (var item in items)
        {
            if (createdObjects.ContainsKey(item.name) && createdObjects.ContainsKey(item.parentName))
            {
                Transform child = createdObjects[item.name];
                Transform parent = createdObjects[item.parentName];
                child.SetParent(parent, false);

                // Eðer panele buton eklediysek ve panelde layout yoksa ekleyelim
                if (parent.GetComponent<VerticalLayoutGroup>() == null && parent.GetComponent<HorizontalLayoutGroup>() == null && parent.GetComponent<Canvas>() == null)
                {
                    VerticalLayoutGroup vlg = parent.gameObject.AddComponent<VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.MiddleCenter;
                    vlg.childControlHeight = false;
                    vlg.childControlWidth = false;
                    vlg.spacing = 20;
                }
            }
        }

        Debug.Log("Gemini arayüzü oluþturdu!");
    }
}