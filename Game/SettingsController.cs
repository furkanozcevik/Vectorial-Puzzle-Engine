using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public Slider volumeSlider;
    public Toggle vibrationToggle;

    void Start()
    {
        // Kaydedilmiþ ayarlarý yükle
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        int vibStatus = PlayerPrefs.GetInt("Vibration", 1); // 1: Açýk, 0: Kapalý

        if (volumeSlider)
        {
            volumeSlider.value = savedVol;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (vibrationToggle)
        {
            vibrationToggle.isOn = (vibStatus == 1);
            vibrationToggle.onValueChanged.AddListener(SetVibration);
        }
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value; // Global ses seviyesi
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetVibration(bool isOn)
    {
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
        // Mobil titreþim kontrolü (Örnek)
        // if(isOn) Handheld.Vibrate(); 
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://seninsiten.com/gizlilik");
    }
}