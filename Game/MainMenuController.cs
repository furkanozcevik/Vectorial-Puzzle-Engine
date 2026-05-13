using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    public RectTransform screenContainer; // Saða sola kayan ana kutu
    public GameObject settingsPanel;
    public GameObject marketPanel;

    [Header("Level Grid Yöneticisi")]
    public LevelGridManager levelGridManager;

    // Hedef Pozisyonlar (Canvas geniþliðine göre ayarlanýr)
    private float screenWidth;

    void Start()
    {
        // Ekran geniþliðini al (Örn: 1920)
        screenWidth = GetComponent<Canvas>().GetComponent<RectTransform>().rect.width;
    }

    // --- ANA MENÜ BUTONLARI ---

    public void OnClick_PlayContinue()
    {
        // En son kalýnan leveli aç
        int savedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (savedLevel > 1000) savedLevel = 1000;
        SceneManager.LoadScene("Level " + savedLevel);
    }

    public void OnClick_Settings() { settingsPanel.SetActive(true); }
    public void OnClick_Market() { marketPanel.SetActive(true); }

    public void ClosePopups()
    {
        settingsPanel.SetActive(false);
        marketPanel.SetActive(false);
    }

    // --- NAVIGASYON (KAYDIRMA) ---

    public void GoToThemes() // Sað Ok
    {
        // Ekraný sola kaydýr (X = -1920) -> Temalar gelir
        StartCoroutine(SlideTo(-screenWidth));
    }

    public void GoToMain() // Sol Ok (Temalardan Geri)
    {
        // Ekraný merkeze al (X = 0) -> Ana Menü gelir
        StartCoroutine(SlideTo(0));
    }

    public void GoToLevelGrid(int themeIndex) // Tema Butonlarýna Basýnca
    {
        string themeName = "";
        switch (themeIndex)
        {
            case 0: themeName = "KLASÝK TEMA"; break;
            case 1: themeName = "KIÞ TEMASI"; break;
            case 2: themeName = "LAV TEMASI"; break;
            case 3: themeName = "TOKSÝK TEMA"; break;
        }

        // Grid Yöneticisine "Þu temayý yükle" de
        levelGridManager.OpenTheme(themeIndex, themeName);

        // Ekraný daha da sola kaydýr (X = -3840) -> Level Grid gelir
        StartCoroutine(SlideTo(-screenWidth * 2));
    }

    public void BackToThemes() // Level Grid'den Geri
    {
        StartCoroutine(SlideTo(-screenWidth));
    }

    // --- KAYDIRMA EFEKTÝ ---
    IEnumerator SlideTo(float targetX)
    {
        float duration = 0.4f;
        float timer = 0f;
        Vector2 startPos = screenContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, 0);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(0, 1, t);
            screenContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        screenContainer.anchoredPosition = targetPos;
    }
}