using UnityEngine;
using System.Collections;

public class InteractionManager : MonoBehaviour
{
    [Header("Katman Ayarlar�")]
    public LayerMask draggableLayer; // Hem TA�INIR hem D�NER (Ayna, B�l�c� vb.)
    public LayerMask rotatableLayer; // Sadece D�NER (Sabit �pu�lar� vb.) <-- BU SATIR EKLEND�
    public LayerMask groundLayer;    // Zemin (Placement)

    [Header("�arp��ma & Hareket")]
    public LayerMask obstacleLayers; // Engeller (Duvar, di�er objeler)
    public float gridSize = 1.0f;
    public float smoothSpeed = 20f;
    public float rotationDuration = 0.2f;

    [Header("Harita S�n�rlar�")]
    public Vector2 minBounds = new Vector2(0, 0);
    public Vector2 maxBounds = new Vector2(20, 20);

    [Header("G�rsel Efektler")]
    public GameObject gridOverlay;

    // Durum De�i�kenleri
    private GameObject selectedObject;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Vector3 initialClickPosition;
    private float clickTime;
    private float dragThreshold = 0.2f;

    private Vector3 currentVelocity; // SmoothDamp i�in

    // O anki objenin izinleri
    private bool canCurrentMove = false;
    private bool canCurrentRotate = false;

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // --- 1. TIKLAMA (SE�ME) ---
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Hem Draggable hem Rotatable katmanlar�n� ayn� anda tar�yoruz
            // "|" i�areti "VEYA" demektir.
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, draggableLayer | rotatableLayer))
            {
                selectedObject = hit.collider.gameObject;

                // --- KATMAN KONTROL� (K�ML�K SORGULAMA) ---
                int objLayer = selectedObject.layer;

                // Matematiksel olarak objenin katman�, Draggable maskesinin i�inde var m�?
                // (Bitwise i�lem: Katman maskesiyle objenin katman�n� kar��la�t�r�r)
                if (((1 << objLayer) & draggableLayer) != 0)
                {
                    // DRAGGABLE KATMANI: Her �eyi yapabilir
                    canCurrentMove = true;
                    canCurrentRotate = true;
                }
                else if (((1 << objLayer) & rotatableLayer) != 0)
                {
                    // ROTATABLE KATMANI: Sadece d�nebilir, hareket edemez
                    canCurrentMove = false;
                    canCurrentRotate = true;
                }

                // T�klama verilerini kaydet
                initialClickPosition = Input.mousePosition;
                clickTime = Time.time;
                dragOffset = Vector3.zero; // Merkezden tut

                isDragging = true;
                if (gridOverlay != null) gridOverlay.SetActive(true);
            }
        }

        // --- 2. S�R�KLEME ---
        if (Input.GetMouseButton(0) && isDragging && selectedObject != null)
        {
            // Sadece hareket izni varsa ta��
            if (canCurrentMove && Vector3.Distance(Input.mousePosition, initialClickPosition) > dragThreshold)
            {
                MoveObject();
            }
        }

        // --- 3. BIRAKMA ---
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // T�klama (K�sa s�reli bas��) ise
            if (Vector3.Distance(Input.mousePosition, initialClickPosition) < dragThreshold && (Time.time - clickTime) < 0.3f)
            {
                // Sadece d�n�� izni varsa d�nd�r
                if (canCurrentRotate)
                {
                    RotateObject();
                }
            }
            else if (selectedObject != null && canCurrentMove)
            {
                // S�r�kleme bittiyse tam kareye oturt
                SnapToGridFinal();
            }

            isDragging = false;
            selectedObject = null;
            if (gridOverlay != null) gridOverlay.SetActive(false);
        }
    }

    void MoveObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            float surfaceHeight = hit.point.y;
            float objectHeightOffset = 0.5f;
            Vector3 rawPos = hit.point;

            float x = Mathf.Round(rawPos.x / gridSize) * gridSize;
            float z = Mathf.Round(rawPos.z / gridSize) * gridSize;

            // S�n�rland�rma (Clamp)
            x = Mathf.Clamp(x, minBounds.x, maxBounds.x);
            z = Mathf.Clamp(z, minBounds.y, maxBounds.y);

            float y = surfaceHeight + objectHeightOffset;
            Vector3 targetPos = new Vector3(x, y, z);

            Collider myCollider = selectedObject.GetComponent<Collider>();
            if (myCollider != null) myCollider.enabled = false;

            // �arp��ma Kontrol� (Engel var m�?)
            bool isBlocked = Physics.CheckSphere(targetPos, 0.45f, obstacleLayers);

            if (myCollider != null) myCollider.enabled = true;

            if (!isBlocked)
            {
                // Yumu�ak Hareket (SmoothDamp)
                selectedObject.transform.position = Vector3.SmoothDamp(
                    selectedObject.transform.position,
                    targetPos,
                    ref currentVelocity,
                    smoothSpeed * 0.01f // SmoothTime olarak kulland���m�z i�in k���k say� laz�m
                );
            }
        }
    }

    void SnapToGridFinal()
    {
        Vector3 currentPos = selectedObject.transform.position;
        float x = Mathf.Round(currentPos.x / gridSize) * gridSize;
        float z = Mathf.Round(currentPos.z / gridSize) * gridSize;

        x = Mathf.Clamp(x, minBounds.x, maxBounds.x);
        z = Mathf.Clamp(z, minBounds.y, maxBounds.y);

        float y = currentPos.y;
        selectedObject.transform.position = new Vector3(x, y, z);
    }

    void RotateObject()
    {
        if (selectedObject != null)
        {
            StartCoroutine(SmoothRotate(selectedObject, 45f));
        }
    }

    IEnumerator SmoothRotate(GameObject obj, float angle)
    {
        Quaternion startRotation = obj.transform.rotation;
        Quaternion targetRotation = obj.transform.rotation * Quaternion.Euler(0, angle, 0);
        float timeElapsed = 0;

        while (timeElapsed < rotationDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / rotationDuration;
            obj.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }
        obj.transform.rotation = targetRotation;
    }
}