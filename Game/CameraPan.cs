using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [Header("Kontrol Durumu")]
    public bool isControlActive = true; // LevelManager buray� kapat�p a�acak

    [Header("H�z Ayarlar�")]
    public float panSpeed = 25f;        // Kayd�rma h�z�
    public float rotationSpeed = 5f;    // D�nme h�z�
    public float zoomSpeed = 15f;       // Yak�nla�ma h�z�

    [Header("Yumu�akl�k Ayarlar�")]
    public float positionSmoothTime = 0.2f; // Pozisyon gecikmesi (0.1 = H�zl�, 0.3 = A��r)
    public float rotationSmoothTime = 10f;  // D�n�� yumu�akl���

    [Header("�ak��ma �nleme (�nemli)")]
    public LayerMask draggableLayer;    // Ayna/B�l�c� katman� (Kamera buraya t�klay�nca oynamamal�)

    [Header("S�n�rlar (Limitler)")]
    public float minXAngle = 30f;       // En fazla ne kadar yere e�ilsin?
    public float maxXAngle = 90f;       // En fazla ne kadar tepeye ��ks�n?
    public float minZoomY = 5f;         // En yak�n mesafe
    public float maxZoomY = 40f;        // En uzak mesafe
    public Vector2 minBounds = new Vector2(-50, -50); // Harita s�n�r� (Sol-Alt)
    public Vector2 maxBounds = new Vector2(50, 50);   // Harita s�n�r� (Sa�-�st)

    // Hedef De�erler
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // SmoothDamp i�in h�z referans�
    private Vector3 currentVelocity;

    // S�r�kleme durumu
    private bool isPanning = false;

    void Start()
    {
        // Ba�lang��ta kameran�n oldu�u yeri hedef olarak al
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Kontrol a��ksa girdileri oku
        if (isControlActive)
        {
            HandleInput();
        }

        // Kontrol kapal� olsa bile yumu�ak duru� i�in bunu her zaman �al��t�r
        MoveSmoothly();
    }

    void HandleInput()
    {
        // --- 1. SOL TIK: KAYDIRMA (PAN) ---

        // T�klama ba�lad���nda kontrol et: Alt�m�zda obje var m�?
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // E�er mouse'un alt�nda "Draggable" bir obje YOKSA, kameray� hareket ettir.
            if (!Physics.Raycast(ray, Mathf.Infinity, draggableLayer))
            {
                isPanning = true;
            }
        }

        // T�klama bitti�inde hareketi kes
        if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }

        // S�r�kleme i�lemi
        if (Input.GetMouseButton(0) && isPanning)
        {
            // Mouse hareketlerini al (Ters �evirerek �ekme hissi veriyoruz)
            float h = -Input.GetAxis("Mouse X");
            float v = -Input.GetAxis("Mouse Y");

            // Kameran�n bakt��� y�ne g�re hareket et (Y�ksekli�i de�i�tirme)
            Vector3 moveDir = (transform.forward * v) + (transform.right * h);
            moveDir.y = 0;

            targetPosition += moveDir * panSpeed * Time.deltaTime;
        }

        // --- 2. SA� TIK: D�ND�RME (ORBIT) ---
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            Vector3 currentRot = targetRotation.eulerAngles;
            float newY = currentRot.y + mouseX;
            float newX = currentRot.x - mouseY;

            // A��y� s�n�rla (�rn: 30 ile 90 derece aras�)
            newX = Mathf.Clamp(newX, minXAngle, maxXAngle);

            targetRotation = Quaternion.Euler(newX, newY, 0);
        }

        // --- 3. ZOOM (TEKERLEK) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Bakt���m�z y�ne do�ru yakla�/uzakla�
            Vector3 zoomDir = transform.forward * scroll * zoomSpeed;
            Vector3 potentialPos = targetPosition + zoomDir;

            // Y�kseklik s�n�rlar�n� a�m�yorsa onayla
            if (potentialPos.y > minZoomY && potentialPos.y < maxZoomY)
            {
                targetPosition = potentialPos;
            }
        }

        // --- 4. HAR�TA SINIRLAMASI (CLAMP) ---
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.y);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.x, maxBounds.y);
    }

    void MoveSmoothly()
    {
        // Pozisyon i�in SmoothDamp (Yaylanma etkisi)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);

        // Rotasyon i�in Slerp (K�resel yumu�ak ge�i�)
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothTime);
    }

    // Edit�rde s�n�rlar� g�rmek i�in
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, 0, (minBounds.y + maxBounds.y) / 2);
        Vector3 size = new Vector3(maxBounds.x - minBounds.x, 1, maxBounds.y - minBounds.y);
        Gizmos.DrawWireCube(center, size);
    }
}