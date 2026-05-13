using UnityEngine;
using System.Collections;

public class ObjectPulse : MonoBehaviour
{
    [Header("Efekt Ayarları")]
    public float pulseScale = 1.2f;   // Ne kadar büyüsün? (1.2 katı)
    public float duration = 0.3f;     // Ne kadar sürsün? (Hızlı tepki için 0.3 ideal)

    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    void Start()
    {
        // Oyun başında orijinal boyutunu hafızaya al
        originalScale = transform.localScale;
    }

    public void PlayPulse()
    {
        // Eğer zaten şişiyorsa durdur, baştan başlat (Seri vuruşlarda takılmasın)
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        // Boyutu hemen sıfırla
        transform.localScale = originalScale;

        currentCoroutine = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float halfDuration = duration / 2f;
        float timer = 0f;

        // 1. ŞİŞME (Orijinal -> 1.2x)
        Vector3 targetScale = originalScale * pulseScale;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // 2. İNME (1.2x -> Orijinal)
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Garanti düzeltme
        transform.localScale = originalScale;
    }
}