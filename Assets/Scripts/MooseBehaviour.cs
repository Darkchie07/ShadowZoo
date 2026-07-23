using UnityEngine;

public class MooseBehaviour : AnimalBehaviourBase
{
    public float grazeChance = 0.2f;
    public float grazeDuration = 2f;
    float grazeTimer;

    void Awake()
    {
        enterSpeed = 1.5f;
        wanderSpeed = 0.9f;
        wanderRadius = 1.5f;
    }

    protected override void Wander()
    {
        if (grazeTimer > 0f)
        {
            grazeTimer -= Time.deltaTime;
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }
        base.Wander();
        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f && Random.value < grazeChance)
            grazeTimer = grazeDuration; // berhenti kayak lagi makan rumput
    }
}