using UnityEngine;

public class ElevatorBlock : MonoBehaviour, ILaserInteractable
{
    public void OnLaserHit(LaserBullet bullet, RaycastHit hit)
    {
        // 1. Asansörün merkezini referans al
        Vector3 center = transform.position;

        // 2. Mermiyi asansörün tam ÇATISINA (Y+1) ve MERKEZÝNE (X,Z) ýþýnla
        // (Böylece saðdan soldan yamuk gelse bile ortalanýr)
        Vector3 targetPos = new Vector3(center.x, center.y + 1f, center.z);

        bullet.transform.position = targetPos;

        // --- DEÐÝÞÝKLÝK BURADA ---
        // Eskiden: bullet.transform.position += bullet.transform.forward * 1.1f;
        // Yeni: Sadece 0.1f kadar çok az ileri itiyoruz (Kendi collider'ýndan kurtulsun yeter).
        // Böylece hemen önündeki karede duran Bölücüyü atlamaz.

        bullet.transform.position += bullet.transform.forward * 0.1f;
    }
}