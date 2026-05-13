using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public AudioClip fireSound;

    private bool hasFired = false;

    void Update()
    {
        // Space'e basýldý VE Henüz ateþ edilmedi mi?
        if (Input.GetKeyDown(KeyCode.Space) && !hasFired)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        hasFired = true; // KÝLÝTLE

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            LaserBullet bulletScript = newBullet.GetComponent<LaserBullet>();
            if (bulletScript != null) bulletScript.bulletPrefab = this.bulletPrefab;
            if (fireSound != null) AudioSource.PlayClipAtPoint(fireSound, firePoint.position);
        }
    }

    // LevelManager çaðýrýr
    public void ResetGun()
    {
        hasFired = false; // KÝLÝDÝ AÇ
    }
}