using UnityEngine;

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
    protected Animator animator; 

    protected Vector3 baseEuler;         
    protected Vector3 baseForwardFlat;   
    protected float currentYaw;         

    public virtual void Initialize(Vector3 targetInsideScreen, Vector3 prefabEulerAngles)
    {
        entryTarget = targetInsideScreen;
        wanderCenter = targetInsideScreen;
        hasEntered = false;
        animator = GetComponentInChildren<Animator>();

        baseEuler = prefabEulerAngles; 
        transform.eulerAngles = baseEuler;

        Vector3 fwd = Quaternion.Euler(baseEuler) * Vector3.forward;
        fwd.y = 0f;
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
        wanderTarget = wanderCenter + new Vector3(offset.x, 0f, offset.y);
    }

    protected void MoveTo(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f; 
        if (dir.sqrMagnitude > 0.0001f)
        {
            Vector3 desiredDir = dir.normalized;
            float targetYaw = Vector3.SignedAngle(baseForwardFlat, desiredDir, Vector3.up);
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(baseEuler.x, baseEuler.y + currentYaw, baseEuler.z);
        }

        Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
        transform.position = Vector3.MoveTowards(transform.position, flatTarget, speed * Time.deltaTime);
        AnimateStep(dir.magnitude);
    }

    protected virtual float EnterSpeedCurrent() => enterSpeed;
    protected virtual float WanderSpeedCurrent() => wanderSpeed;

    protected virtual void AnimateStep(float distanceRemaining)
    {
        if (animator != null) animator.SetFloat("Speed", distanceRemaining);
    }
}