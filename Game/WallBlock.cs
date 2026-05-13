using UnityEngine;

public class WallBlock : MonoBehaviour, ILaserInteractable
{
    public AudioClip destroySound;

    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        if (destroySound) AudioSource.PlayClipAtPoint(destroySound, hit.point);
        Destroy(bullet.gameObject);
    }
}
