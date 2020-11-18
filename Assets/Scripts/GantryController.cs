using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GantryController : MonoBehaviour
{
    public GameObject AreaClearButton;
    public GameObject FuelButton;
    public GameObject LaunchButton;
    public float LaunchpadSize = 4;
    // Start is called before the first frame update

    private GameObject Rocket;
    private bool areaCleared;
    private GameObject[] rockets;
    private float dist;
    private AudioSource speaker;

    void Start()
    {
        areaCleared = false;
        rockets = GameObject.FindGameObjectsWithTag("LaunchableRocket");
        Rocket = null;
        dist = float.MaxValue;
        speaker = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!areaCleared)
        {
            rockets = GameObject.FindGameObjectsWithTag("LaunchableRocket");
            foreach (GameObject g in rockets)
            {
                if (g == Rocket)
                {
                    if ((g.transform.position - this.transform.position).magnitude > LaunchpadSize)
                    {
                        Rocket = null;
                        dist = float.MaxValue;
                    }
                }
                else if ((g.transform.position - this.transform.position).magnitude < LaunchpadSize)
                {
                    if (Rocket)
                    {
                        if ((g.transform.position - this.transform.position).magnitude < dist)
                        {
                            Rocket = g;
                            dist = (g.transform.position - this.transform.position).magnitude;
                        }
                    }
                    else
                    {
                        Rocket = g;
                        dist = (g.transform.position - this.transform.position).magnitude;
                    }
                }
            }
            if (Rocket)
            {
                AreaClearButton.SendMessage("RocketReady", true);
                FuelButton.SendMessage("RocketReady", true);
            }
            else
            {
                AreaClearButton.SendMessage("RocketReady", false);
                FuelButton.SendMessage("RocketReady", false);
            }
        }
    }

    void AreaCleared()
    {
        areaCleared = true;
    }

    void Launch()
    {
        StartCoroutine("CorLaunch");
    }

    IEnumerator CorLaunch()
    {
        Rocket.SendMessage("Launch");
        Debug.Log("Launch command sent to rocket");
        AreaClearButton.SendMessage("RocketLaunched", true);
        FuelButton.SendMessage("RocketLaunched", true);
        yield return null;
    }
}
