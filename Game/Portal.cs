using UnityEngine;

public class Portal : MonoBehaviour, ILaserInteractable
{
    [Header("Baðlantý")]
    public Portal linkedPortal; // Diðer uçtaki portal

    [Header("Çýkýþ Noktasý")]
    public Transform spawnPoint; // Merminin fýrlayacaðý nokta

    // Animasyon scriptine referans
    private ObjectPulse pulseEffect;

    void Start()
    {
        // Kendi üzerindeki animasyon bileþenini al
        pulseEffect = GetComponent<ObjectPulse>();
    }

    // Interface'den gelen çarpýþma fonksiyonu
    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        // 1. GÝRÝÞ ANÝMASYONU: Merminin çarptýðý bu portalý þiþir
        if (pulseEffect != null)
        {
            pulseEffect.PlayPulse();
        }

        if (linkedPortal != null)
        {
            // 2. ÇIKIÞ ANÝMASYONU: Diðer uçtaki portalý da þiþir
            // (Böylece oyuncu merminin nereden çýktýðýný göz ucuyla yakalar)
            ObjectPulse exitPulse = linkedPortal.GetComponent<ObjectPulse>();
            if (exitPulse != null)
            {
                exitPulse.PlayPulse();
            }

            // 3. IÞINLANMA ÝÞLEMÝ
            bullet.TeleportBullet(this, linkedPortal);
        }
        else
        {
            // Baðlantý yoksa mermiyi yok et
            Destroy(bullet.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 1f);
        }
    }
}