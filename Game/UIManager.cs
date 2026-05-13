using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Paneller")]
    public GameObject winPanel;

    [Header("Animasyon Ayarları")]
    public float fadeDuration = 0.4f; // Kapanış biraz daha hızlı olsun

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        PreparePanel(winPanel);
    }

    void PreparePanel(GameObject panel)
    {
        if (panel == null) return;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        panel.SetActive(false);
    }

    // --- AÇMA FONKSİYONU ---
    public void ShowWinPanel()
    {
        StartCoroutine(AnimatePanel(winPanel, true));
    }

    // --- KAPATMA FONKSİYONU (YENİ) ---
    public void HideWinPanel()
    {
        StartCoroutine(AnimatePanel(winPanel, false));
    }

    // Tek bir fonksiyon hem açmayı hem kapatmayı yönetir (Kod tekrarını önler)
    IEnumerator AnimatePanel(GameObject panel, bool show)
    {
        if (panel == null) yield break;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        if (show)
        {
            panel.SetActive(true); // Açarken hemen aktif et
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.interactable = false; // Kapatırken tıklamayı hemen engelle
            cg.blocksRaycasts = false;
        }

        float timer = 0f;
        Vector3 startScale = show ? Vector3.one * 0.8f : Vector3.one;
        Vector3 endScale = show ? Vector3.one : Vector3.one * 0.8f;
        float startAlpha = show ? 0f : 1f;
        float endAlpha = show ? 1f : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / fadeDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothProgress);
            panel.transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);

            yield return null;
        }

        // Animasyon bitişi
        cg.alpha = endAlpha;
        panel.transform.localScale = endScale;

        if (!show)
        {
            panel.SetActive(false); // Kapatma işlemi bittiyse objeyi gizle
        }
    }
}