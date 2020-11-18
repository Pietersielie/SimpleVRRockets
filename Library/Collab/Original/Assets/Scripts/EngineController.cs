using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineController : MonoBehaviour
{
    public ParticleSystem[] PFX;
    public int SpecificImpulse = 220;
    public float Thrust = 24;
    // Start is called before the first frame update
    private bool running;
    private Rigidbody rb;
    private AudioSource speaker;
    private float Burntime = 50;
    void Start()
    {
        running = false;
        rb = GetComponent<Rigidbody>();
        speaker = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
       if (running)
       {
            Burntime -= Time.deltaTime;
            if (Burntime <= 0)
            {
                running = false;
                StopPFX();
            }
       }
    }

    void FixedUpdate()
    {
        
    }

    void Launch()
    {
        StartCoroutine("Startup");
    }

    IEnumerator Startup()
    {
        foreach (ParticleSystem f in PFX)
        {
            f.Play();
        }
        speaker.Play();
        Debug.Log("Ignition started");
        yield return new WaitForSeconds(3);
        running = true;
    }

    IEnumerator StopPFX()
    {
        speaker.Stop();
        foreach (ParticleSystem f in PFX)
        {
            f.Stop();
        }
        yield return null;
    }

    void SetBurnTime(float fuel)
    {
        Burntime = (SpecificImpulse * fuel) / Thrust;
        Debug.Log("Burntime set to " + Burntime);
    }

    void AddThrust()
    {
        rb.AddForce(transform.forward * Thrust, ForceMode.Force);
    }
}
