using UnityEngine;

public class BirdBehaviour : AnimalBehaviourBase
{
    public float flapFrequency = 6f;
    public float bobAmplitude = 0.15f;
    float bobTimer;

    void Awake()
    {
        enterSpeed = 4.5f; 
        wanderSpeed = 3f;
        wanderRadius = 2.5f;
    }

    protected override void AnimateStep(float distanceRemaining)
    {
        bobTimer += Time.deltaTime * flapFrequency;
        Vector3 p = transform.position;
        p.y += Mathf.Sin(bobTimer) * bobAmplitude * Time.deltaTime;
        transform.position = p;
        base.AnimateStep(distanceRemaining);
    }
}