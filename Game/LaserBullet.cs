using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float speed = 10f;
    public float maxLifeTime = 10f;

    [Header("Bölünme Ayarlarý (Splitter Okur)")]
    public int generation = 0;       // Kaçýncý nesil mermi?
    public int maxGenerations = 10;  // Sonsuz döngü engeli

    [Header("Referanslar (Diðer Objeler Kullanýr)")]
    public GameObject bulletPrefab;  // Bölücü, yeni mermi üretmek için bunu kullanýr
    public LayerMask placementLayer; // Zemin (Tile) kontrolü için katman

    [Header("Ses Efektleri (Diðer Objeler Kullanýr)")]
    public AudioClip bounceSound;
    public AudioClip splitSound;
    public AudioClip destroySound;

    void Start()
    {
        // 1. Level Manager'a mermiyi kaydet (Bitti mi kontrolü için)
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterBullet();

        // 2. Ömür süresi dolunca yok ol
        Destroy(gameObject, maxLifeTime);

        // 3. Trail Renderer süresini hýza göre ayarla (2 birimlik kuyruk)
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.time = 2.0f / speed;
    }

    void OnDestroy()
    {
        // Level Manager'dan kaydý sil
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterBullet();
    }

    void Update()
    {
        MoveBullet();
    }

    void MoveBullet()
    {
        float stepDistance = speed * Time.deltaTime;

        // 1. ZEMÝN KONTROLÜ
        if (!CheckPathValidity(stepDistance))
        {
            Destroy(gameObject);
            return;
        }

        // --- YENÝ EKLENEN KISIM: ZEMÝN TÜRÜ KONTROLÜ ---
        CheckFloorType();
        // ------------------------------------------------

        // 2. ÇARPIÞMA KONTROLÜ
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, stepDistance))
        {
            HandleCollision(hit);
        }
        else
        {
            transform.Translate(Vector3.forward * stepDistance);
        }
    }

    // --- ZEMÝN TÜRÜ KONTROLÜ ---
    void CheckFloorType()
    {
        // Merminin tam altýný kontrol et
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        RaycastHit hit;

        // Sadece Placement (Zemin) katmanýna bak
        if (Physics.Raycast(ray, out hit, 2f, placementLayer))
        {
            // Eðer bastýðýmýz zemin "Straightener" (veya Düzeltici) etiketine sahipse
            if (hit.collider.CompareTag("Straightener"))
            {
                // Fonksiyona karenin merkezini de gönderiyoruz ki mermiyi ortalasýn
                SnapToNearest45(hit.collider.transform.position);
            }
        }
    }

    // --- YENÝ FONKSÝYON: EN YAKIN 45 DERECEYE YUVARLA ---
    void SnapToNearest45(Vector3 tileCenter)
    {
        float currentY = transform.eulerAngles.y;

        // MATEMATÝK: Açýyý 45'e böl, yuvarla ve tekrar 45 ile çarp.
        // Örnek: 42 -> 45 olur. 10 -> 0 olur. 88 -> 90 olur.
        float snappedY = Mathf.Round(currentY / 45f) * 45f;

        // Titreþimi önlemek için: Eðer zaten açýmýz doðruysa (fark çok azsa) iþlem yapma
        if (Mathf.Abs(Mathf.DeltaAngle(currentY, snappedY)) < 0.1f) return;

        // 1. Yeni açýyý uygula
        transform.rotation = Quaternion.Euler(0, snappedY, 0);

        // 2. POZÝSYON HÝZALAMA (Çok Önemli)
        // Mermi yamuk gelip düzeldiðinde, karenin kenarýndan gitmesin diye
        // onu karenin tam merkezine (X ve Z) çekiyoruz.
        transform.position = new Vector3(tileCenter.x, transform.position.y, tileCenter.z);
    }

    // Zemin (Tile) var mý kontrolü
    bool CheckPathValidity(float distanceToCheck)
    {
        Vector3 nextPosition = transform.position + (transform.forward * distanceToCheck);

        // Merminin gideceði yerin 1 birim yukarýsýndan aþaðýya ýþýn atýyoruz
        Ray checkRay = new Ray(nextPosition + Vector3.up, Vector3.down);

        // Sadece "Placement" katmanýný kontrol et
        if (Physics.Raycast(checkRay, 5f, placementLayer))
        {
            return true; // Yol var
        }
        return false; // Yol yok (Boþluk)
    }

    void HandleCollision(RaycastHit hit)
    {
        // --- INTERFACE SÝSTEMÝ ---
        // Çarptýðýmýz objenin "ILaserInteractable" özelliði (sözleþmesi) var mý?
        // (Ayna, Duvar, Portal, Asansör, Bölücü, Hedef... Hepsi bunu kullanýr)
        ILaserInteractable interactable = hit.collider.GetComponent<ILaserInteractable>();

        if (interactable != null)
        {
            // Varsa, kontrolü ona veriyoruz.
            interactable.OnLaserHit(this, hit);
        }
        else
        {
            // Etkileþimsiz bir þeye çarptýysa (Örn: Yanlýþlýkla zemine deðdiyse) yok et.
            Destroy(gameObject);
        }
    }

    // --- YARDIMCI FONKSÝYON: PORTAL IÞINLANMA ---
    // Portal scripti bu fonksiyonu çaðýrýr.
    public void TeleportBullet(Portal entrance, Portal exit)
    {
        // 1. Çýkýþ Noktasý (SpawnPoint) yoksa objenin merkezini al
        Transform exitTransform = exit.spawnPoint != null ? exit.spawnPoint : exit.transform;

        // Pozisyonu ayarla
        transform.position = exitTransform.position;

        // 2. AÇI HESABI (Giriþ açýsýný koruyarak çýkýþa aktar)
        // Merminin giriþ portalýna göre yerel yönünü bul
        Vector3 localDirection = entrance.transform.InverseTransformDirection(transform.forward);

        // 180 derece çevir (Çünkü portaldan çýkýyoruz)
        Vector3 flippedDirection = Quaternion.Euler(0, 180, 0) * localDirection;

        // Yeni yönü dünya koordinatýna çevirip uygula
        transform.forward = exitTransform.TransformDirection(flippedDirection);

        // 3. Çýkýþta kendi portalýna çarpmamasý için hafifçe ileri it
        transform.position += transform.forward * 0.2f;
    }
}