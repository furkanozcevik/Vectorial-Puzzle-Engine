using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelGridManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Transform gridHolder;
    public GameObject buttonPrefab;
    public Text pageTitleText;
    public Button nextBtn, prevBtn;
    public Text themeTitleText;

    [Header("Arka Plan Sistemi")]
    public Image backgroundImage;     // Deðiþecek olan UI Image (Canvas arkasýndaki)
    public Sprite[] themeBackgrounds; // 4 Adet Resim (0:Klasik, 1:Kýþ, 2:Lav, 3:Toksik)

    [Header("Ayarlar")]
    public int levelsPerPage = 50;

    private int currentThemeStartIndex = 1;
    private int currentPage = 0;
    private string currentThemeName;
    private List<LevelButton> buttonPool = new List<LevelButton>();

    void Start()
    {
        GeneratePool();
    }

    void GeneratePool()
    {
        foreach (Transform child in gridHolder) Destroy(child.gameObject);
        for (int i = 0; i < levelsPerPage; i++)
        {
            GameObject obj = Instantiate(buttonPrefab, gridHolder);
            LevelButton lb = obj.GetComponent<LevelButton>();
            buttonPool.Add(lb);
        }
    }

    public void OpenTheme(int themeIndex, string themeName)
    {
        currentThemeName = themeName;
        currentThemeStartIndex = (themeIndex * 250) + 1; // Her tema 250 level
        currentPage = 0;

        // --- ARKA PLAN DEÐÝÞTÝRME ---
        if (backgroundImage != null && themeBackgrounds.Length > themeIndex)
        {
            backgroundImage.sprite = themeBackgrounds[themeIndex];
        }

        UpdateGrid();
    }

    public void NextPage()
    {
        if (currentPage < 4)
        {
            currentPage++;
            UpdateGrid();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateGrid();
        }
    }

    void UpdateGrid()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (pageTitleText) pageTitleText.text = $"Sayfa {currentPage + 1}/5";
        if (themeTitleText) themeTitleText.text = currentThemeName;

        int pageStartLevel = currentThemeStartIndex + (currentPage * levelsPerPage);

        for (int i = 0; i < levelsPerPage; i++)
        {
            int realLevelNumber = pageStartLevel + i;
            buttonPool[i].SetupButton(realLevelNumber, unlockedLevel);
        }

        if (prevBtn) prevBtn.interactable = (currentPage > 0);
        if (nextBtn) nextBtn.interactable = (currentPage < 4);
    }
}