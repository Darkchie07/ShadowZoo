using UnityEngine;

public class DeerBehaviour : AnimalBehaviourBase
{
    public float alertPauseChance = 0.15f;
    public float alertPauseDuration = 1.2f;
    float pauseTimer;
    float hopTimer;

    void Awake()
    {
        enterSpeed = 3f;
        wanderSpeed = 1.5f;
        wanderRadius = 2f;
    }

    protected override void Wander()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }
        base.Wander();
        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f && Random.value < alertPauseChance)
            pauseTimer = alertPauseDuration; // berhenti waspada sebentar kayak rusa asli
    }

    protected override void AnimateStep(float distanceRemaining)
    {
        hopTimer += Time.deltaTime * 8f;
        Vector3 p = transform.position;
        p.y += Mathf.Abs(Mathf.Sin(hopTimer)) * 0.05f * Time.deltaTime; // lompatan kecil tiap langkah
        transform.position = p;
        base.AnimateStep(distanceRemaining);
    }
}