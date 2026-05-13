using UnityEngine;

// Bu bir "Sözleþme"dir. Bunu kullanan her obje "OnLaserHit" fonksiyonunu içermek ZORUNDADIR.
public interface ILaserInteractable
{
    void OnLaserHit(LaserBullet bullet, RaycastHit hitInfo);
}
