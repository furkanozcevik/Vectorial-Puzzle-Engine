using UnityEngine;

public class ObjectResetter : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // Oyun başladığı anki (Level oluştuğundaki) konumu kaydet
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void ResetToStart()
    {
        // Başlangıç konumuna geri dön
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}