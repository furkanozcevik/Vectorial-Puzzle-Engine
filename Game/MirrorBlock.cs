using UnityEngine;

public class MirrorBlock : MonoBehaviour, ILaserInteractable
{
    [Header("Çıkış Noktası")]
    public Transform spawnPoint;

    // Efekt Scriptine Referans
    private ObjectPulse pulseEffect;

    void Start()
    {
        pulseEffect = GetComponent<ObjectPulse>();
    }

    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        // 1. Animasyonu Oynat (Şişme Efekti)
        if (pulseEffect != null) pulseEffect.PlayPulse();

        // 2. Yansıma Yönünü Hesapla
        Vector3 incomingDir = bullet.transform.forward;
        Vector3 reflectDir = Vector3.Reflect(incomingDir, hit.normal);

        // 3. Çıkış Pozisyonu
        Vector3 finalPos = (spawnPoint != null) ? spawnPoint.position : hit.point;

        // 4. Yeni Mermiyi Ateşle
        SpawnBullet(bullet, finalPos, Quaternion.LookRotation(reflectDir));

        // 5. Eski Mermiyi Yok Et
        Destroy(bullet.gameObject);
    }

    void SpawnBullet(LaserBullet originalBullet, Vector3 pos, Quaternion rot)
    {
        GameObject newObj = Instantiate(originalBullet.bulletPrefab, pos, rot);
        LaserBullet newScript = newObj.GetComponent<LaserBullet>();

        if (newScript != null)
        {
            newScript.bulletPrefab = originalBullet.bulletPrefab;
            newScript.placementLayer = originalBullet.placementLayer;
            newScript.generation = originalBullet.generation;
        }
    }
}