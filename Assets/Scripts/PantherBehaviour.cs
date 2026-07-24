using UnityEngine;

public class PantherBehaviour : AnimalBehaviourBase
{
    public float sprintChance = 0.1f;
    public float sprintSpeedMultiplier = 3f;
    public float sprintDuration = 0.8f;
    float sprintTimer;

    void Awake()
    {
        enterSpeed = 2f;
        wanderSpeed = 0.8f;
        wanderRadius = 2f;
    }

    protected override float WanderSpeedCurrent()
        => sprintTimer > 0f ? wanderSpeed * sprintSpeedMultiplier : wanderSpeed;

    protected override void Wander()
    {
        if (sprintTimer > 0f) sprintTimer -= Time.deltaTime;
        base.Wander();
        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f && sprintTimer <= 0f && Random.value < sprintChance)
        {
            sprintTimer = sprintDuration;
            PickNewWanderTarget();
        }
    }
}