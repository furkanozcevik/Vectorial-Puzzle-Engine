using UnityEngine;

public class Target : MonoBehaviour, ILaserInteractable
{
    [Header("Hedef Ayarları")]
    public AudioClip hitSound; // Vurulma Sesi

    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        // 1. SESİ ÇAL (Obje yok olsa bile ses çalsın diye PlayClipAtPoint kullanıyoruz)
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.0f);
        }

        // 2. Hedefi Kapat (Vuruldu)
        gameObject.SetActive(false);

        // 3. Mermiyi Yok Et
        Destroy(bullet.gameObject);
    }

    public void ResetTarget()
    {
        gameObject.SetActive(true);
    }
}