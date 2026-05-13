using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public Text levelText;
    public GameObject lockIcon;
    public GameObject checkIcon;

    private int targetLevel;
    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClicked);
    }

    // ÝSÝM DEÐÝÞÝKLÝÐÝ: 'Setup' yerine 'SetupButton' yaptýk.
    public void SetupButton(int level, int unlockedLevel)
    {
        targetLevel = level;
        if (levelText) levelText.text = targetLevel.ToString();

        // 1000 Level sýnýrý
        if (targetLevel > 1000)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Kilit Mantýðý
        if (targetLevel <= unlockedLevel)
        {
            btn.interactable = true;
            if (lockIcon) lockIcon.SetActive(false);
            if (targetLevel < unlockedLevel && checkIcon) checkIcon.SetActive(true);
            else if (checkIcon) checkIcon.SetActive(false);
        }
        else
        {
            btn.interactable = false;
            if (lockIcon) lockIcon.SetActive(true);
            if (checkIcon) checkIcon.SetActive(false);
        }
    }

    void OnClicked()
    {
        // Sahne ismi "Level 50" gibiyse burayý kendine göre düzenle
        SceneManager.LoadScene("Level " + targetLevel);
    }
}