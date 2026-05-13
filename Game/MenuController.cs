using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public RectTransform screensContainer; // Saða sola kayacak ana kutu

    [Header("Ayarlar")]
    public float slideDuration = 0.5f; // Kayma süresi

    private Vector2 mainMenuPos;
    private Vector2 levelSelectPos;

    void Start()
    {
        // Pozisyonlarý belirle
        // Main Menu (0,0)'da durur.
        mainMenuPos = Vector2.zero;

        // Level Select ekraný gelmesi için kutuyu Sola (-1920) kaydýrmalýyýz.
        // Screen.width kullanarak her telefona uyumlu yapýyoruz.
        float screenWidth = GetComponent<Canvas>().GetComponent<RectTransform>().rect.width;
        levelSelectPos = new Vector2(-screenWidth, 0);
    }

    // --- BUTON FONKSÝYONLARI ---

    public void OnClick_Next() // Sað Oka basýnca
    {
        StopAllCoroutines();
        StartCoroutine(SlideTo(levelSelectPos));
    }

    public void OnClick_Back() // Sol Oka basýnca
    {
        StopAllCoroutines();
        StartCoroutine(SlideTo(mainMenuPos));
    }

    public void OnClick_Play()
    {
        // Direkt son kalýnan leveli açmak istersen:
        int savedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        SceneManager.LoadScene("Level " + savedLevel);
    }

    public void OnClick_Settings() { Debug.Log("Ayarlar açýldý (Henüz boþ)"); }
    public void OnClick_Shop() { Debug.Log("Market açýldý (Henüz boþ)"); }

    // --- KAYDIRMA ANÝMASYONU ---
    IEnumerator SlideTo(Vector2 targetPos)
    {
        float timer = 0f;
        Vector2 startPos = screensContainer.anchoredPosition;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float t = timer / slideDuration;
            // SmoothStep ile yumuþak geçiþ
            t = Mathf.SmoothStep(0, 1, t);

            screensContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        screensContainer.anchoredPosition = targetPos;
    }
}