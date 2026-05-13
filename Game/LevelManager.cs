using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Müzik Ayarları")]
    public AudioClip backgroundMusic;
    private AudioSource musicSource;

    [Header("Sahne Ayarları")]
    public string menuSceneName = "Menu";

    // Takip Listeleri
    private List<Target> allTargets = new List<Target>();
    private List<ObjectResetter> allResettables = new List<ObjectResetter>();

    private Gun playerGun;
    private CameraPan mainCamera;

    private int activeBulletCount = 0;
    private bool decisionMade = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.4f;
    }

    void Start()
    {
        if (backgroundMusic != null) { musicSource.clip = backgroundMusic; musicSource.Play(); }

        Time.timeScale = 1f;
        playerGun = FindFirstObjectByType<Gun>();
        mainCamera = FindFirstObjectByType<CameraPan>();

        FindTargets();
        FindResettables();
    }

    void FindTargets()
    {
        allTargets.Clear();
        GameObject[] targetObjs = GameObject.FindGameObjectsWithTag("Target");
        foreach (GameObject obj in targetObjs)
        {
            Target t = obj.GetComponent<Target>();
            if (t != null) allTargets.Add(t);
        }
    }

    void FindResettables()
    {
        allResettables.Clear();
        ObjectResetter[] objs = FindObjectsByType<ObjectResetter>(FindObjectsSortMode.None);
        allResettables.AddRange(objs);
    }

    public void RegisterBullet() { activeBulletCount++; }

    public void UnregisterBullet()
    {
        activeBulletCount--;
        if (activeBulletCount <= 0 && !decisionMade) StartCoroutine(CheckGameStatus());
    }

    IEnumerator CheckGameStatus()
    {
        // Son merminin sönmesi için bekle
        yield return new WaitForSeconds(0.5f);

        if (activeBulletCount > 0) yield break;

        decisionMade = true;

        bool activeTargetExists = false;
        foreach (Target t in allTargets)
        {
            if (t.gameObject.activeSelf)
            {
                activeTargetExists = true;
                break;
            }
        }

        if (activeTargetExists)
        {
            // --- KAYBETME DURUMU (Otomatik Devam) ---
            Debug.Log("Vurulamayan hedefler var. Tekrar atış hakkı veriliyor (Objeler taşınmıyor).");

            // Oyuncuya başarısız olduğunu hissettirmek için çok kısa bekle
            yield return new WaitForSeconds(0.5f);

            // SADECE ATIŞI SIFIRLA (Objeleri elleme)
            ResetForNextShot();
        }
        else
        {
            // --- KAZANMA DURUMU ---
            Debug.Log("KAZANDIN! Win Paneli açılıyor...");
            yield return new WaitForSeconds(0.5f);
            WinLevel();
        }
    }

    // YENİ FONKSİYON: Mermi bitince otomatik çalışır. 
    // Aynaların yerini DEĞİŞTİRMEZ, sadece mermiyi ve hedefleri yeniler.
    public void ResetForNextShot()
    {
        activeBulletCount = 0;
        decisionMade = false;
        StopAllCoroutines();

        // 1. Hedefleri Canlandır (Ki tekrar vurmayı deneyebilsin)
        foreach (Target t in allTargets) t.ResetTarget();

        // 2. Silahı Doldur
        if (playerGun != null) playerGun.ResetGun();

        // NOT: allResettables (Aynalar) burada resetlenmiyor! Oyuncu düzenini koruyor.
    }

    // MANUEL RESTART BUTONU: Oyuncu "Ben bu işi batırdım, en başa dön" derse çalışır.
    public void SoftRestart()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideWinPanel();

        // Önce temel atış hazırlığını yap (Hedefleri aç, silahı doldur)
        ResetForNextShot();

        // EKSTRA OLARAK: Objeleri (Ayna, Bölücü) başlangıç yerlerine ışınla
        foreach (ObjectResetter r in allResettables)
        {
            if (r != null) r.ResetToStart();
        }

        if (mainCamera != null) mainCamera.isControlActive = true;

        Debug.Log("Tam Reset Atıldı (Objeler eski yerine döndü).");
    }

    void WinLevel()
    {
        if (mainCamera != null) mainCamera.isControlActive = false;
        if (UIManager.Instance != null) UIManager.Instance.ShowWinPanel();
        else LoadNextLevel();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (currentLevelIndex >= unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel + 1);
            PlayerPrefs.Save();
        }
    }

    public void LoadNextLevel()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideWinPanel();
        int currentIdx = SceneManager.GetActiveScene().buildIndex;
        int nextIdx = currentIdx + 1;
        if (nextIdx < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(nextIdx);
        else SceneManager.LoadScene(menuSceneName);
    }

    public void LoadMenu()
    {
        if (UIManager.Instance != null) UIManager.Instance.HideWinPanel();
        SceneManager.LoadScene(menuSceneName);
    }

    // UI butonuna bağlı olan fonksiyon (Tam Reset Yapar)
    public void RestartLevel() => SoftRestart();
}