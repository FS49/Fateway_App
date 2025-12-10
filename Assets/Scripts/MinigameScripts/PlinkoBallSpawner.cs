using UnityEngine;

public class PlinkoBallSpawner : MonoBehaviour
{
    [Header("Ball")]
    public GameObject ballPrefab;          // Dein Ball-Prefab (mit Rigidbody2D + CircleCollider2D)
    public float spawnZ = 0f;              // Z-Ebene (meist 0 in 2D)

    [Header("Optionales Spawn-Limit")]
    public Collider2D spawnArea;           // (Optional) Bereich, in dem gespawnt werden darf (z.B. BoxCollider2D als Rahmen)

    [Header("Vorschau (Optional)")]
    public SpriteRenderer ghostPreview;    // (Optional) ein halbtransparentes Sprite, das der Maus folgt
    public bool showGhost = true;

    private bool hasSpawned = false;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (ghostPreview) ghostPreview.gameObject.SetActive(showGhost);
    }

    void Update()
    {
        if (hasSpawned) 
        {
            if (ghostPreview) ghostPreview.gameObject.SetActive(false);
            return;
        }

        // Maus-/Touchposition in Welt
        Vector3 world = GetPointerWorldPosition();

        // Im optionalen Spawn-Bereich clampen
        if (spawnArea != null)
        {
            world = ClampToColliderBounds(world, spawnArea);
        }

        // Vorschau folgen lassen
        if (ghostPreview)
        {
            ghostPreview.transform.position = new Vector3(world.x, world.y, spawnZ);
            ghostPreview.gameObject.SetActive(showGhost);
        }

        // Klick/Touch: genau einmal spawnen
        if (PointerDownThisFrame())
        {
            SpawnBall(world);
            hasSpawned = true;
        }
    }

    Vector3 GetPointerWorldPosition()
    {
        // Maus
        if (Input.mousePresent)
        {
            Vector3 screen = Input.mousePosition;
            screen.z = Mathf.Abs(cam.transform.position.z); // 2D-Setup: Distanz zur Kamera
            return cam.ScreenToWorldPoint(screen);
        }

        // Touch
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            Vector3 screen = t.position;
            screen.z = Mathf.Abs(cam.transform.position.z);
            return cam.ScreenToWorldPoint(screen);
        }

        // Fallback: Mitte
        return Vector3.zero;
    }

    bool PointerDownThisFrame()
    {
        return Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    void SpawnBall(Vector3 worldPos)
    {
        Vector3 pos = new Vector3(worldPos.x, worldPos.y, spawnZ);
        Instantiate(ballPrefab, pos, Quaternion.identity);
    }

    Vector3 ClampToColliderBounds(Vector3 p, Collider2D col)
    {
        Bounds b = col.bounds;
        float x = Mathf.Clamp(p.x, b.min.x, b.max.x);
        float y = Mathf.Clamp(p.y, b.min.y, b.max.y);
        return new Vector3(x, y, p.z);
    }

    // Falls du später erneut spawnen willst, kannst du diese Methode aufrufen
    public void AllowNextSpawn()
    {
        hasSpawned = false;
        if (ghostPreview) ghostPreview.gameObject.SetActive(showGhost);
    }
}
