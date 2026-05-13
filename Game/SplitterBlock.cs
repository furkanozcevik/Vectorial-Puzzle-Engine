using UnityEngine;
using System.Collections.Generic;

public class SplitterBlock : MonoBehaviour, ILaserInteractable
{
    [Header("Çıkış Noktaları")]
    public List<Transform> allSpawnPoints;

    // Efekt Scriptine Referans
    private ObjectPulse pulseEffect;

    void Start()
    {
        pulseEffect = GetComponent<ObjectPulse>();
    }

    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        // 1. Animasyonu Oynat
        if (pulseEffect != null) pulseEffect.PlayPulse();

        // 2. Sonsuz döngü kontrolü
        if (bullet.generation >= bullet.maxGenerations)
        {
            Destroy(bullet.gameObject);
            return;
        }

        // 3. Açıları Hesapla (Grid sistemine uygun 45 derece)
        float incomingAngle = bullet.transform.eulerAngles.y;
        float angle1 = Mathf.Round((incomingAngle + 45f) / 45f) * 45f;
        float angle2 = Mathf.Round((incomingAngle - 45f) / 45f) * 45f;

        Vector3 dir1 = Quaternion.Euler(0, angle1, 0) * Vector3.forward;
        Vector3 dir2 = Quaternion.Euler(0, angle2, 0) * Vector3.forward;

        // 4. En uygun çıkış noktalarını bul
        Vector3 pos1 = GetBestSpawnPos(dir1);
        Vector3 pos2 = GetBestSpawnPos(dir2);

        // 5. Mermileri Oluştur
        SpawnBullet(bullet, pos1, dir1);
        SpawnBullet(bullet, pos2, dir2);

        // 6. Eski Mermiyi Yok Et
        Destroy(bullet.gameObject);
    }

    Vector3 GetBestSpawnPos(Vector3 targetDirection)
    {
        if (allSpawnPoints == null || allSpawnPoints.Count == 0) return transform.position;

        Transform bestPoint = null;
        float maxDot = -Mathf.Infinity;

        foreach (Transform point in allSpawnPoints)
        {
            if (point == null) continue;
            Vector3 pointDir = (point.position - transform.position).normalized;
            float dot = Vector3.Dot(pointDir, targetDirection);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestPoint = point;
            }
        }
        return (bestPoint != null) ? bestPoint.position : transform.position;
    }

    void SpawnBullet(LaserBullet originalBullet, Vector3 pos, Vector3 dir)
    {
        GameObject newObj = Instantiate(originalBullet.bulletPrefab, pos, Quaternion.LookRotation(dir));
        LaserBullet newScript = newObj.GetComponent<LaserBullet>();

        if (newScript != null)
        {
            newScript.bulletPrefab = originalBullet.bulletPrefab;
            newScript.placementLayer = originalBullet.placementLayer;
            newScript.generation = originalBullet.generation + 1;
        }
    }
}