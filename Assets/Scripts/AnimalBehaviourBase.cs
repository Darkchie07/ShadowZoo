using UnityEngine;

/// <summary>
/// Base state machine: Enter (dari luar layar jalan ke titik dalam layar) -> Wander (muter-muter di area itu).
/// Tiap class turunan cuma override speed & AnimateStep buat gaya gerak yang beda-beda.
/// </summary>
public abstract class AnimalBehaviourBase : MonoBehaviour
{
    [Header("Movement")]
    public float enterSpeed = 2f;
    public float wanderSpeed = 1f;
    public float wanderRadius = 1.5f;
    public float rotationSpeed = 8f;

    protected Vector3 entryTarget;
    protected Vector3 wanderCenter;
    protected Vector3 wanderTarget;
    protected bool hasEntered = false;
    protected Animator animator; // opsional, kalau prefab punya Animator dengan param "Speed"

    protected Vector3 baseEuler;         // Euler asli PREFAB ASSET (X/Z apa adanya, gak lewat decompose quaternion)
    protected Vector3 baseForwardFlat;   // arah hadap prefab di yaw=0, diproyeksi ke bidang tanah
    protected float currentYaw;          // offset yaw yang kita track manual (derajat), ditambahin ke baseEuler.y

    /// <summary>Dipanggil sesaat setelah Instantiate, kasih tau titik tujuan + Euler asli prefab asset.</summary>
    public virtual void Initialize(Vector3 targetInsideScreen, Vector3 prefabEulerAngles)
    {
        entryTarget = targetInsideScreen;
        wanderCenter = targetInsideScreen;
        hasEntered = false;
        animator = GetComponentInChildren<Animator>();

        baseEuler = prefabEulerAngles; // ambil langsung dari ASSET prefab, bukan dari transform.eulerAngles objek ini
        transform.eulerAngles = baseEuler; // pastiin Inspector nunjukin persis angka yang sama kayak prefab pas spawn

        Vector3 fwd = Quaternion.Euler(baseEuler) * Vector3.forward;
        fwd.y = 0f;
        // kalau forward prefab kebetulan lurus ke atas/bawah (fwd flat = nol), fallback ke world forward
        baseForwardFlat = fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector3.forward;

        currentYaw = 0f;
        PickNewWanderTarget();
    }

    protected virtual void Update()
    {
        if (!hasEntered)
            MoveTowardsEntry();
        else
            Wander();
    }

    protected virtual void MoveTowardsEntry()
    {
        MoveTo(entryTarget, EnterSpeedCurrent());
        if (Vector3.Distance(transform.position, entryTarget) < 0.15f)
        {
            hasEntered = true;
            OnEntered();
        }
    }

    protected virtual void OnEntered() => PickNewWanderTarget();

    protected virtual void Wander()
    {
        MoveTo(wanderTarget, WanderSpeedCurrent());
        if (Vector3.Distance(transform.position, wanderTarget) < 0.15f)
            PickNewWanderTarget();
    }

    protected void PickNewWanderTarget()
    {
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        wanderTarget = wanderCenter + new Vector3(offset.x, 0f, offset.y); // sebar di bidang X-Z (tanah), Y ikut wanderCenter
    }

    protected void MoveTo(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f; // arah jalan cuma di bidang tanah (X-Z); Y dipakai AnimateStep buat bobbing/tinggi
        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 desiredDir = dir.normalized;
            // yaw yang dibutuhkan supaya baseForwardFlat (arah hadap asli prefab) ngarah ke desiredDir
            float targetYaw = Vector3.SignedAngle(baseForwardFlat, desiredDir, Vector3.up);
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationSpeed * Time.deltaTime);

            // Unity nyusun Euler dgn urutan Z-X-Y (Y di luar/world-space), jadi nambah currentYaw
            // ke baseEuler.y itu SETARA MATEMATIS sama muter di world-Y lewat quaternion.
            // Bedanya: X & Z di Inspector tetap kebaca persis kayak prefab, gak ke-decompose ulang jadi angka lain.
            transform.eulerAngles = new Vector3(baseEuler.x, baseEuler.y + currentYaw, baseEuler.z);
        }

        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        transform.position = Vector3.MoveTowards(transform.position, flatTarget, speed * Time.deltaTime);
        AnimateStep(dir.magnitude);
    }

    protected virtual float EnterSpeedCurrent() => enterSpeed;
    protected virtual float WanderSpeedCurrent() => wanderSpeed;

    /// <summary>Hook buat gaya gerak unik tiap kelas (bobbing, scale pulse, dsb).</summary>
    protected virtual void AnimateStep(float distanceRemaining)
    {
        if (animator != null) animator.SetFloat("Speed", distanceRemaining);
    }
}