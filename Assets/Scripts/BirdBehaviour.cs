using UnityEngine;

public class BirdBehaviour : AnimalBehaviourBase
{
    public float flapFrequency = 6f;
    public float bobAmplitude = 0.15f;
    float bobTimer;

    void Awake()
    {
        enterSpeed = 4.5f;   // masuk layar cepet, kesan terbang
        wanderSpeed = 3f;
        wanderRadius = 2.5f; // jelajah area lebih luas
    }

    protected override void AnimateStep(float distanceRemaining)
    {
        bobTimer += Time.deltaTime * flapFrequency;
        Vector3 p = transform.position;
        p.y += Mathf.Sin(bobTimer) * bobAmplitude * Time.deltaTime; // naik-turun kayak kepakan sayap
        transform.position = p;
        base.AnimateStep(distanceRemaining);
    }
}