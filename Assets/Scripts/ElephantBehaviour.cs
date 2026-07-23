using UnityEngine;

public class ElephantBehaviour : AnimalBehaviourBase
{
    public float stompFrequency = 1.5f;
    public float scalePulse = 0.03f;
    Vector3 baseScale;
    float stepTimer;

    void Awake()
    {
        enterSpeed = 1.2f;  // lambat & berat
        wanderSpeed = 0.7f;
        wanderRadius = 1f;
    }

    protected override void OnEntered()
    {
        base.OnEntered();
        baseScale = transform.localScale;
    }

    protected override void AnimateStep(float distanceRemaining)
    {
        if (baseScale == Vector3.zero) baseScale = transform.localScale;
        stepTimer += Time.deltaTime * stompFrequency;
        float s = 1f + Mathf.Sin(stepTimer) * scalePulse;
        transform.localScale = baseScale * s; // efek "berat" tiap langkah
        base.AnimateStep(distanceRemaining);
    }
}