using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemExtraControlls : MonoBehaviour
{
    private ParticleSystem ps;
    private float lifeTime;

    public AnimationCurve emissionOverAge;
    public AnimationCurve sizeOverAge;
    // Start is called before the first frame update
    void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        lifeTime = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.EmissionModule emission = ps.emission;
        ParticleSystem.MainModule main = ps.main;
        float age = (Time.time - lifeTime) / ps.main.duration;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(emissionOverAge.Evaluate(age));
        main.startSize = new ParticleSystem.MinMaxCurve(sizeOverAge.Evaluate(age));
    }
}
